using System.Runtime.CompilerServices;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

/// <summary>
/// 玩家移動控制器 - 處理角色移動、跳躍、俯衝和攻擊
/// 支持地面和空中的物理交互
/// </summary>
public class PlayerMove : NetworkBehaviour
{
    #region 序列化欄位

    [Header("移動設置")]
    [SerializeField] private float moveSpeed = 8.0f;
    [SerializeField] private float sprintSpeed = 12f;
    [SerializeField] private float accelTime = 0.1f;        // 地面加速
    [SerializeField] private float decelTime = 0.02f;       // 地面煞車
    [SerializeField] private float airAccelTime = 0.15f;    // 空中加速
    [SerializeField] private float airDecelTime = 0.5f;     // 空中慣性
    [SerializeField] private float rotationSmoothTime = 0.12f;

    [Header("跳躍 / 重力 / 擊退")]
    [SerializeField] private float jumpHeight = 3.0f;
    [SerializeField] private float jumpTimeout = 0.10f;
    [SerializeField] private float gravity = -15.0f;
    [SerializeField] private float damping = 6f;
    [SerializeField] private float terminalVelocity = 53.0f;

    [Header("地面檢測")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float groundedRadius = 0.28f;
    [SerializeField] private float groundedOffset = -0.14f;

    [Header("飛撲")]
    [SerializeField] private NetworkAnimator lungeAnim;
    [SerializeField] private float lungeForwardSpeed = 10f;    // 水平飛撲速度
    [SerializeField] private float lungeUpwardSpeed = 8f;      // 向上彈射速度
    [SerializeField] private float lungeDuration = 0.5f;       // 飛撲持續時間
    [SerializeField] private float lungeCooldown = 1f;       // 飛撲冷卻時間

    [Header("攻擊")]
    [SerializeField] private NetworkAnimator attackAnim;
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private float attackCooldown = 0.5f;

    #endregion

    #region 私有變數

    // 組件引用
    private CharacterController controller;
    private StarterAssetsInputs input;
    private GameObject mainCamera;

    // 移動狀態
    private float speed;
    private float speedVelocity;
    private float verticalVelocity;
    private float targetRotation = 0.0f;
    private float rotationVelocity;

    // 地面狀態
    private bool grounded = true;
    private float jumpTimeoutDelta;

    // 飛撲狀態
    private bool islungeing = false;
    private bool canLunge = true;  // 是否可以飛撲
    private Vector3 currentlungeDir;
    private float lungeDurationTimer;
    private float lungeCooldownTimer;

    // 攻擊狀態
    private bool isAttacking = false;
    private float attackDurationTimer;
    private float attackCooldownTimer;

    // 擊退狀態
    private bool isKnockback;
    private Vector3 externalVelocity;

    // 移動平台
    private Transform currentPlatform = null;
    private Vector3 lastPlatformPosition;
    private Vector3 platformDelta;

    #endregion

    #region Unity 生命週期

    private void Start()
    {
        InitializeComponents();
    }

    private void Update()
    {
        if (!IsOwner) return;

        JumpAndGravity();
        GroundedCheck();
        Move();
        lunge();
        Attack();
    }

    private void LateUpdate()
    {
        if (!IsOwner || currentPlatform == null) return;
        HandleMovingPlatform();
    }

    #endregion

    #region 初始化

    private void InitializeComponents()
    {
        StartCoroutine(WaitForMainCameraAndAssign());

        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (input == null)
            input = GetComponent<StarterAssetsInputs>();

        jumpTimeoutDelta = jumpTimeout;
        groundedOffset = (controller.height / 2f) + controller.center.y;
    }

    private System.Collections.IEnumerator WaitForMainCameraAndAssign()
    {
        float timeout = 3f;
        float timer = 0f;

        while (mainCamera == null && timer < timeout)
        {
            // 尋找有 MainCamera tag 的物件
            GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
            if (cam != null && cam.GetComponent<Camera>() != null)
            {
                mainCamera = cam;
                break;
            }

            yield return null;
            timer += Time.unscaledDeltaTime;
        }

        if (mainCamera == null)
        {
            Debug.LogWarning("[PlayerMove] 初始化時未找到攝影機，將在移動時自動查找");
        }
    }

    #endregion

    #region 地面檢測

    private void GroundedCheck()
    {
        // 檢查腳下是否有地面
        Vector3 spherePosition = new Vector3(
            transform.position.x,
            transform.position.y - groundedOffset,
            transform.position.z
        );
        grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);

        // 檢測移動平台
        DetectMovingPlatform();
    }

    private void DetectMovingPlatform()
    {
        RaycastHit hit;
        if (Physics.SphereCast(
            transform.position,
            groundedRadius,
            Vector3.down,
            out hit,
            groundedOffset + 0.5f,
            groundLayers,
            QueryTriggerInteraction.Ignore))
        {
            var movingPlatform = hit.collider.GetComponentInParent<MovingPlatform>();

            if (movingPlatform != null)
            {
                Transform actualMovingTransform = movingPlatform.GetPlatformTransform();

                if (currentPlatform != actualMovingTransform)
                {
                    currentPlatform = actualMovingTransform;
                    lastPlatformPosition = currentPlatform.position;
                }
            }
            else
            {
                currentPlatform = null;
            }
        }
        else
        {
            currentPlatform = null;
        }
    }

    private void HandleMovingPlatform()
    {
        Vector3 currentPlatformPos = currentPlatform.position;
        platformDelta = currentPlatformPos - lastPlatformPosition;

        if (platformDelta.sqrMagnitude > 0)
        {
            controller.Move(platformDelta);
        }

        lastPlatformPosition = currentPlatformPos;
    }

    #endregion

    #region 移動

    private void Move()
    {
        // 確保攝影機引用有效
        if (mainCamera == null)
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        // 處理擊退
        if (isKnockback)
        {
            HandleKnockback();
            return;
        }

        // 計算目標速度
        float targetSpeed = input.sprint ? sprintSpeed : moveSpeed;
        if (input.move == Vector2.zero)
            targetSpeed = 0.0f;

        // 更新速度
        if (!islungeing)
        {
            UpdateSpeed(targetSpeed);
        }

        // 計算移動方向
        Vector3 inputDirection = new Vector3(input.move.x, 0.0f, input.move.y).normalized;

        // 旋轉角色
        if (input.move != Vector2.zero)
        {
            RotateCharacter(inputDirection);
        }

        // 執行移動
        if (!islungeing)
        {
            Vector3 targetDirection = transform.forward;
            Vector3 playerMotion = (targetDirection * speed + new Vector3(0, verticalVelocity, 0)) * Time.deltaTime;
            controller.Move(playerMotion);
        }
    }

    private void HandleKnockback()
    {
        controller.Move(externalVelocity * Time.deltaTime + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

        // 水平擊退逐漸衰減
        externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, Time.deltaTime * damping);

        // 接近 0 時結束擊退
        if (externalVelocity.magnitude < 0.1f)
        {
            externalVelocity = Vector3.zero;
            isKnockback = false;
        }
    }

    private void UpdateSpeed(float targetSpeed)
    {
        // 地面急停
        if (grounded && targetSpeed < 0.01f && speed < 0.5f)
        {
            speed = 0f;
            speedVelocity = 0f;
        }
        else
        {
            float targetSmoothTime = (targetSpeed > speed)
                ? (grounded ? accelTime : airAccelTime)
                : (grounded ? decelTime : airDecelTime);

            speed = Mathf.SmoothDamp(speed, targetSpeed, ref speedVelocity, targetSmoothTime);
        }
    }

    private void RotateCharacter(Vector3 inputDirection)
    {
        if (mainCamera == null) return;

        targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                         mainCamera.transform.eulerAngles.y;
        float rotation = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetRotation,
            ref rotationVelocity,
            rotationSmoothTime
        );
        transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
    }

    #endregion

    #region 跳躍與重力

    private void JumpAndGravity()
    {
        if (grounded)
        {
            // 飛撲時不重置垂直速度
            if (!islungeing)
            {
                // 防止垂直速度無限累積
                if (verticalVelocity < 0.0f)
                {
                    verticalVelocity = -2f; // 保持一點向下速度確保穩定貼地
                }
            }

            // 執行跳躍
            if (input.jump && jumpTimeoutDelta <= 0.0f)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // 跳躍冷卻
            if (jumpTimeoutDelta >= 0.0f)
            {
                jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // 防止兔子跳
            jumpTimeoutDelta = jumpTimeout;
            input.jump = false;
        }

        // 應用重力
        if (verticalVelocity < terminalVelocity)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    #endregion

    #region 飛撲

    private void lunge()
    {
        // 著地且不在飛撲時恢復飛撲能力
        if (grounded && !islungeing)
        {
            canLunge = true;
        }

        lungeDurationTimer -= Time.deltaTime;
        lungeCooldownTimer -= Time.deltaTime;

        HandlelungeMovement();

        if (!input.lunge || islungeing) return;

        // 檢查冷卻
        if (lungeCooldownTimer > 0)
        {
            input.lunge = false;
            return;
        }

        // 檢查是否可以飛撲
        if (!canLunge)
        {
            input.lunge = false;
            return;
        }

        // 執行飛撲並消耗飛撲能力
        canLunge = false;
        Startlunge();

        input.lunge = false;
    }

    private void Startlunge()
    {
        islungeing = true;
        lungeDurationTimer = lungeDuration;
        lungeCooldownTimer = lungeCooldown;
        currentlungeDir = transform.forward;

        // 給予向上的初始速度，形成拋物線
        verticalVelocity = lungeUpwardSpeed;

        if (lungeAnim != null && IsOwner)
        {
            lungeAnim.SetTrigger("isLunge");
        }
    }

    private void HandlelungeMovement()
    {
        if (!islungeing) return;

        // 水平方向移動（保持飛撲方向）
        Vector3 horizontalMovement = currentlungeDir * lungeForwardSpeed * Time.deltaTime;

        // 垂直方向受重力影響（形成拋物線）
        Vector3 verticalMovement = new Vector3(0, verticalVelocity, 0) * Time.deltaTime;

        // 合併水平和垂直移動
        controller.Move(horizontalMovement + verticalMovement);

        // 更新速度顯示
        speed = lungeForwardSpeed;
        speedVelocity = 0f;

        // 檢查是否結束飛撲：時間到
        if (lungeDurationTimer <= 0f)
        {
            islungeing = false;
        }
    }

    #endregion

    #region 攻擊

    private void Attack()
    {
        attackDurationTimer -= Time.deltaTime;
        attackCooldownTimer -= Time.deltaTime;

        if (isAttacking && attackDurationTimer <= 0f)
        {
            isAttacking = false;
        }

        if (input.attack && !isAttacking)
        {
            // 檢查冷卻
            if (attackCooldownTimer > 0)
            {
                input.attack = false;
                return;
            }

            // 執行攻擊
            StartAttack();
        }

        input.attack = false;
    }

    private void StartAttack()
    {
        isAttacking = true;
        attackDurationTimer = attackDuration;
        attackCooldownTimer = attackCooldown;

        if (attackAnim != null && IsOwner)
        {
            attackAnim.SetTrigger("isAttack");
        }
    }

    #endregion

    #region 公開方法

    /// <summary>
    /// 彈射（跳板使用）
    /// </summary>
    public void Launch(float launchVelocity)
    {
        verticalVelocity = launchVelocity;
        jumpTimeoutDelta = jumpTimeout;
    }

    /// <summary>
    /// 應用擊退力
    /// </summary>
    [ClientRpc]
    public void ApplyKnockbackClientRpc(Vector3 force)
    {
        if (!IsOwner) return;

        externalVelocity = force;
        isKnockback = true;
        Launch(force.y);
    }

    /// <summary>
    /// 調整減速時間
    /// </summary>
    [ClientRpc]
    public void AdjustDecelTimeClientRpc(float adjustment, ClientRpcParams rpcParams = default)
    {
        if (!IsOwner) return;

        decelTime = Mathf.Max(0.01f, decelTime + adjustment);
        Debug.Log($"[PlayerMove] 目前的減速時間: {decelTime}");
    }

    #endregion

    #region 調試

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (controller != null)
        {
            Vector3 spherePosition = new Vector3(
                transform.position.x,
                transform.position.y - (controller.height / 2f) + controller.center.y,
                transform.position.z
            );
            Gizmos.DrawWireSphere(spherePosition, groundedRadius);
        }
    }

    #endregion
}
