using System.Runtime.CompilerServices;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMove : NetworkBehaviour
{
    /* ========== 移動參數 ========== */
    [SerializeField] private float SprintSpeed = 20f;
    [SerializeField] private float MoveSpeed = 5.0f;
    // 加速&減速速率
    [SerializeField] private float speedAccelerationRate = 10f;
    [SerializeField] private float speedDecelerationRate = 15f;


    /* ========== 跳躍與重力參數 ========== */
    [SerializeField] private float JumpHeight = 2.0f;
    [SerializeField] private float JumpTimeout = 0.10f;
    // 重力
    [SerializeField] private float Gravity = -15.0f;
    // 離地高度補正
    [SerializeField] private float GroundedOffset = 0.9f;
    // 地板圖層(什麼東西算地板)
    [SerializeField] private LayerMask GroundLayers;
    // 確認玩家是否在地板上
    [SerializeField] private bool Grounded = true;
    // 地板檢測半徑
    [SerializeField] private float GroundedRadius = 0.28f;
    // 腳色旋轉速度
    [SerializeField] private float RotationSmoothTime = 0.12f;

    /* ========== 滑鏟參數 ========== */
    [SerializeField] private Animator diveAnim;
    [SerializeField] private float diveDuration = 0.3f;
    [SerializeField] private float diveCooldown = 0.5f;
    [SerializeField] private float diveForce = 10f;

    /* ========== 外力參數 ========== */
    //擊退衰減速度
    [SerializeField] private float Damping = 6f;
    // 外力影響
    private Vector3 externalVelocity;
    // 是否正在被擊退
    private bool isKnockback;

    private float cinemachineTargetYaw;
    private float speed;

    //追蹤旋轉角度
    private float rotationVelocity;
    //追蹤下落角度
    private float verticalVelocity;
    private float jumpTimeoutDelta;
    //最大掉落速度
    private float terminalVelocity = 53.0f;
    private float targetRotation = 0.0f;
    private CharacterController controller;
    private StarterAssetsInputs input;
    private GameObject mainCamera;

    // 滑鏟狀態變數
    private Vector3 currentDiveDir;
    private float diveDurationTimer = 0f;
    private float diveCooldownTimer = 0f;
    private bool isDiving = false;
    private bool canAirDive = true;

    // 閥值
    private const float THRESHOLD = 0.01f;

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
        JumpAndGravity();
        GroundedCheck();
        Move();
        Dive();
        Attack();
    }

    private void GroundedCheck()
    {
        // 在腳色底下設置一個球體檢測玩家是否在地板上
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
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

        float targetSpeed = input.sprint ? SprintSpeed : MoveSpeed;

        if (input.move == Vector2.zero) targetSpeed = 0.0f;

        // 取得水平速度
        float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;
        // 避免抖動(不停地加速減速)
        float speedOffset = 0.1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            float SpeedChangeRate;

            if (targetSpeed > currentHorizontalSpeed)
                SpeedChangeRate = speedAccelerationRate;
            else
                SpeedChangeRate = speedDecelerationRate;
            // 線性插值
            speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed,
                Time.deltaTime * SpeedChangeRate);

            // 限制小數點
            speed = Mathf.Round(speed * 1000f) / 1000f;
        }
        else
        {
            speed = targetSpeed;
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

        // move the player
        controller.Move(targetDirection.normalized * (speed * Time.deltaTime) +
                            new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);
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

        controller.Move(currentDiveDir * diveForce * Time.deltaTime);

        if (diveDurationTimer <= 0f) isDiving = false;
    }

    // 處理左鍵攻擊
    [SerializeField] private Animator attackAnim;
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private float attackCooldown = 0.5f;
    private float attackDurationTimer = 0f;
    private float attackCooldownTimer = 0f;
    private bool isAttacking = false;

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
                Debug.Log("Attack Animation Triggered");
            }
            Debug.Log("Attack Triggered");
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
    public void ReduceSpeedDecelerationRateClientRpc(float reduceRate, ClientRpcParams rpcParams = default)
    {
        if (!IsOwner) return;
        Debug.Log("Reducing speedDecelerationRate by: " + reduceRate);
        speedDecelerationRate = Mathf.Max(0f, speedDecelerationRate - reduceRate);
    }
}
