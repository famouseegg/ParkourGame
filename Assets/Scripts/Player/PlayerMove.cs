using System.Runtime.CompilerServices;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMove : NetworkBehaviour
{
    /* ========== 外部可調整參數 ========== */
    [Header("Movement Settings")]
    [SerializeField] private float MoveSpeed = 8.0f;
    [SerializeField] private float SprintSpeed = 12f;
    [SerializeField] private float accelTime = 0.1f;        // 地面加速
    [SerializeField] private float decelTime = 0.02f;       // 地面煞車
    [SerializeField] private float airAccelTime = 0.15f;    // 空中加速
    [SerializeField] private float airDecelTime = 0.5f;     // 空中慣性
    [SerializeField] private float RotationSmoothTime = 0.12f; // 旋轉平滑時間

    [Header("Jump / Gravity / Knockback")]
    [SerializeField] private float JumpHeight = 3.0f;
    [SerializeField] private float JumpTimeout = 0.10f;
    [SerializeField] private float Gravity = -15.0f;
    [SerializeField] private float Damping = 6f;
    [SerializeField] private float terminalVelocity = 53.0f; // 最大落速

    [Header("Ground Check")]
    [SerializeField] private LayerMask GroundLayers;
    [SerializeField] private float GroundedRadius = 0.28f;
    [SerializeField] private float GroundedOffset = -0.14f;

    [Header("Dive")]
    [SerializeField] private Animator diveAnim;
    [SerializeField] private float diveSpeed = 15f;
    [SerializeField] private float diveDuration = 0.3f;
    [SerializeField] private float diveCooldown = 0.5f;

    [Header("Attack")]
    [SerializeField] private Animator attackAnim;
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private float attackCooldown = 0.5f;

    /* ========== 內部狀態變數 ========== */
    // 組件引用
    private CharacterController controller;
    private StarterAssetsInputs input;
    private GameObject mainCamera;

    // 移動數值追蹤
    private float speed;
    private float speedVelocity;    // SmoothDamp 使用
    private float verticalVelocity;
    private float targetRotation = 0.0f;
    private float rotationVelocity; // SmoothDampAngle 使用

    // 狀態計時器與標記
    private bool Grounded = true;
    private float jumpTimeoutDelta;

    private bool isDiving = false;
    private bool canAirDive = true;
    private Vector3 currentDiveDir;
    private float diveDurationTimer;
    private float diveCooldownTimer;

    private bool isAttacking = false;
    private float attackDurationTimer;
    private float attackCooldownTimer;

    private bool isKnockback;
    private Vector3 externalVelocity;

    // 移動平台相關
    private Transform currentPlatform = null;
    private Vector3 lastPlatformPosition;
    private Vector3 platformDelta;

    private void Start()
    {
        // 只有一個Camara時適用
        if (Camera.main != null)
            mainCamera = Camera.main.gameObject;
        else
            Debug.LogError("場景缺少 Camera.main");

        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (input == null)
            input = GetComponent<StarterAssetsInputs>();

        jumpTimeoutDelta = JumpTimeout;
        GroundedOffset = (controller.height / 2f) + controller.center.y;
    }

    private void Update()
    {
        if (!IsOwner) return;

        JumpAndGravity();
        GroundedCheck();
        Move();
        Dive();
        Attack();
    }

    private void LateUpdate()
    {
        if (!IsOwner || currentPlatform == null) return;

        // 計算位移
        Vector3 currentPlatformPos = currentPlatform.position;
        platformDelta = currentPlatformPos - lastPlatformPosition;

        // 只有在有位移時才移動
        if (platformDelta.sqrMagnitude > 0)
        {
            controller.Move(platformDelta);
        }

        // 更新紀錄
        lastPlatformPosition = currentPlatformPos;
    }

    private void GroundedCheck()
    {
        // 在腳色底下設置一個球體檢測玩家是否在地板上
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

        RaycastHit hit;
        // 使用 SphereCast 偵測腳下
        if (Physics.SphereCast(transform.position, GroundedRadius, Vector3.down, out hit, GroundedOffset + 0.5f, GroundLayers, QueryTriggerInteraction.Ignore))
        {
            // 找到移動腳本
            var movingPlatform = hit.collider.GetComponentInParent<MovingPlatform>();

            if (movingPlatform != null)
            {
                // 鎖定腳本中指定的會動的 platformParent
                Transform actualMovingTransform = movingPlatform.GetPlatformTransform();

                if (currentPlatform != actualMovingTransform)
                {
                    currentPlatform = actualMovingTransform;
                    lastPlatformPosition = currentPlatform.position;
                }
            }
            else
            {
                // 離開移動平台
                currentPlatform = null;
            }
        }
        else
        {
            // 離開地面檢測範圍
            currentPlatform = null;
        }
    }

    private void Move()
    {
        if (isKnockback)
        {
            controller.Move(externalVelocity * Time.deltaTime + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

            // 水平擊退逐漸衰減
            externalVelocity = Vector3.Lerp(
                externalVelocity,
                Vector3.zero,
                Time.deltaTime * Damping
            );

            // 接近 0 時結束擊退
            if (externalVelocity.magnitude < 0.1f)
            {
                externalVelocity = Vector3.zero;
                isKnockback = false;
            }

            // 擊退時無法移動    
            return;
        }

        if (input.sprint) { Debug.Log("Sprint Triggered"); }
        float targetSpeed = input.sprint ? SprintSpeed : MoveSpeed;

        if (input.move == Vector2.zero) targetSpeed = 0.0f;

        if (!isDiving)
        {
            // 地面急停
            if (Grounded && targetSpeed < 0.01f && speed < 0.5f)
            {
                speed = 0f;
                speedVelocity = 0f; // 清空慣性
            }
            else
            {
                float targetSmoothTime = (targetSpeed > speed)
                ? (Grounded ? accelTime : airAccelTime)
                : (Grounded ? decelTime : airDecelTime);

                speed = Mathf.SmoothDamp(speed, targetSpeed, ref speedVelocity, targetSmoothTime);
            }
        }

        // 把vector2 輸入轉為 vector3
        Vector3 inputDirection = new Vector3(input.move.x, 0.0f, input.move.y).normalized;
        // 避免抖動
        if (input.move != Vector2.zero)
        {
            if (mainCamera == null)
            {
                Debug.LogError("PlayerMove: mainCamera is null; cannot rotate player.");
                return;
            }
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity,
                                RotationSmoothTime);

            // 旋轉以面向輸入方向（相對於攝影機位置）
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
        Vector3 playerMotion = (targetDirection * speed + new Vector3(0, verticalVelocity, 0)) * Time.deltaTime;

        // move the player
        if (!isDiving)
        {
            controller.Move(playerMotion);
        }
    }

    private void Dive()
    {
        if (Grounded) canAirDive = true;

        diveDurationTimer -= Time.deltaTime;
        diveCooldownTimer -= Time.deltaTime;

        HandleDiveMovement();

        if (!input.dive || isDiving) return;

        if (diveCooldownTimer > 0)
        {
            input.dive = false;
            return;
        }

        if (Grounded || canAirDive)
        {
            if (!Grounded) canAirDive = false;

            isDiving = true;
            diveDurationTimer = diveDuration;
            diveCooldownTimer = diveCooldown;
            currentDiveDir = transform.forward;

            if (diveAnim != null)
            {
                diveAnim.SetTrigger("isDive");
            }
        }
        input.dive = false;
    }

    private void HandleDiveMovement()
    {
        if (!isDiving) return;

        speed = diveSpeed;
        speedVelocity = 0f;

        controller.Move(currentDiveDir * diveSpeed * Time.deltaTime);

        if (diveDurationTimer <= 0f) { isDiving = false; }
    }

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
            if (attackCooldownTimer > 0)
            {
                input.attack = false;
                return;
            }

            // 啟動攻擊
            isAttacking = true;
            attackDurationTimer = attackDuration;
            attackCooldownTimer = attackCooldown;

            if (attackAnim != null)
            {
                attackAnim.SetTrigger("IsAttack");
            }
        }
        input.attack = false;
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {

            // 防止垂直速度無限累積
            if (verticalVelocity < 0.0f)
            {
                // 留一點向下速度，確保角色：穩定貼在地面、CharacterController 能正確判斷 Grounded
                verticalVelocity = -2f;
            }

            // 玩家有按跳躍鍵 & 跳躍冷卻時間已結束
            if (input.jump && jumpTimeoutDelta <= 0.0f)
            {
                // 下落速度攻式 v = sqrt(h*-2*g) 
                verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            }

            // jump timeout
            if (jumpTimeoutDelta >= 0.0f)
            {
                jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // 防止兔子跳
            jumpTimeoutDelta = JumpTimeout;

            // 防止「按住跳躍鍵」不放
            input.jump = false;
        }

        // 在最大掉落速度到達之前 會呈線性加速
        if (verticalVelocity < terminalVelocity)
        {
            verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    // 繪製碰撞體
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
            Gizmos.DrawWireSphere(spherePosition, GroundedRadius);
        }
    }

    public void Launch(float launchVelocity)
    {
        // 向上的初速
        verticalVelocity = launchVelocity;

        // 重置跳躍冷卻，避免跳板後馬上被判定 grounded
        jumpTimeoutDelta = JumpTimeout;
    }

    [ClientRpc]
    public void ApplyKnockbackClientRpc(Vector3 force)
    {
        if (!IsOwner) return;
        externalVelocity = force;
        isKnockback = true;

        // 垂直擊飛
        Launch(force.y);
    }

    [ClientRpc]
    public void AdjustDecelTimeClientRpc(float adjustment, ClientRpcParams rpcParams = default)
    {
        if (!IsOwner) return;
        decelTime = Mathf.Max(0.01f, decelTime + adjustment);
        Debug.Log("目前的減速時間: " + decelTime);
    }
}
