using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMove : MonoBehaviour
{
    [SerializeField] GameObject mainCamera;
    [SerializeField] GameObject CinemachineCameraTarget;
    [SerializeField] float JumpTimeout = 0.50f;
    [SerializeField] float FallTimeout = 0.15f;
    [SerializeField] float GroundedOffset = -0.14f;
    // 攝影機角度補正
    [SerializeField] float CameraAngleOverride = 0.0f;

    // 視角最高最低限制
    public float TopClamp = 70.0f;
    public float BottomClamp = -30.0f;

     // 地板圖層(什麼東西算地板)
    [SerializeField] LayerMask GroundLayers;
    // 攝影機垂直旋轉限制
    public bool LockCameraPosition = false;

    // 確認玩家是否在地板上
    private bool Grounded = true;
    // 地板檢測半徑
    public float GroundedRadius = 0.28f;
    //水平方向旋轉角（左右轉）
    private float cinemachineTargetPitch;
    // 垂直方向旋轉角（上下看）
    private float cinemachineTargetYaw;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    private CharacterController controller;
    private StarterAssetsInputs input;

    // 閥值
    private const float THRESHOLD = 0.01f;
    private void Start()
    {
        cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        controller = GetComponent<CharacterController>();
        input = GetComponent<StarterAssetsInputs>();
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    private void Update()
    {
        // JumpAndGravity();
        GroundedCheck();
        // Move();
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
            cinemachineTargetPitch += input.look.y;
           
        }
        
        //限制旋轉角度在 360 度以內。
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine 將跟著這一目標物體的旋轉
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + CameraAngleOverride,
        cinemachineTargetYaw, 0.0f);
        Debug.Log(CinemachineCameraTarget.transform);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }


}
