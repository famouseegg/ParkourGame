using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMove : MonoBehaviour
{
    
    [SerializeField] private GameObject CinemachineCameraTarget;
    [SerializeField] float JumpTimeout = 0.50f;
    [SerializeField] private float SprintSpeed = 5.335f;
    [SerializeField] private float MoveSpeed = 2.0f;
    [SerializeField] private float JumpHeight = 1.2f;
    // 重力
    [SerializeField] private float Gravity = -15.0f;
    // 加速&減速
    [SerializeField] private float SpeedChangeRate = 10.0f;
    [SerializeField] private float GroundedOffset = 0.9f;
    // 攝影機角度補正
    [SerializeField] private float CameraAngleOverride = 0.0f;
    // 腳色旋轉速度
    [SerializeField] private float RotationSmoothTime = 0.12f;
     // 地板圖層(什麼東西算地板)
    [SerializeField] private LayerMask GroundLayers;

    // 確認玩家是否在地板上
    [SerializeField] private bool Grounded = true;
    // 視角最高最低限制
    public float TopClamp = 70.0f;
    public float BottomClamp = -30.0f;
    
    // 攝影機垂直旋轉限制
    public bool LockCameraPosition = false;

    
    // 地板檢測半徑
    public float GroundedRadius = 0.28f;
    //水平方向旋轉角（左右轉）
    private float cinemachineTargetPitch;
    // 垂直方向旋轉角（上下看）
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

    // 閥值
    private const float THRESHOLD = 0.01f;
    private void Start()
    {
        // 只有一個Camara時適用
        mainCamera = Camera.main.gameObject;
        cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        controller = GetComponent<CharacterController>();
        input = GetComponent<StarterAssetsInputs>();
        jumpTimeoutDelta = JumpTimeout;
        GroundedOffset = (controller.height / 2f) + controller.center.y;
    }

    private void Update()
    {
        JumpAndGravity();
        GroundedCheck();
        Move();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void GroundedCheck()
    {
        // 在腳色底下設置一個球體檢測玩家是否在地板上
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,QueryTriggerInteraction.Ignore);
    }

    private void CameraRotation()
    {           
        //如果有輸入(設定閥值避免抖動) & 相機未鎖定
        if (input.look.sqrMagnitude >= THRESHOLD && !LockCameraPosition)
        {
            cinemachineTargetYaw += input.look.x;
            cinemachineTargetPitch += -input.look.y;
           
        }
        
        //限制旋轉角度在 360 度以內。
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine 將跟著這一目標物體的旋轉
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + CameraAngleOverride,
        cinemachineTargetYaw, 0.0f);

    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void Move()
    {
        float targetSpeed = input.sprint ? SprintSpeed : MoveSpeed;

        if (input.move == Vector2.zero) targetSpeed = 0.0f;

        // 取得水平速度
        float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;
        // 避免抖動(不停地加速減速)
        float speedOffset = 0.1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
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
}
