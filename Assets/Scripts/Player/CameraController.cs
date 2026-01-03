using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject CinemachineCameraTarget;

    /* ========== 攝影機參數 ========== */
    // 攝影機角度補正
    [SerializeField] private float CameraAngleOverride = 0.0f;
    // 視角最高最低限制
    [SerializeField] private float TopClamp = 70.0f;
    [SerializeField] private float BottomClamp = -30.0f;
    // 攝影機垂直旋轉限制
    [SerializeField] private bool LockCameraPosition = false;
    // 輸入來源
    [SerializeField] private StarterAssetsInputs input;

    // 水平方向旋轉角（左右轉）
    private float cinemachineTargetPitch;
    // 垂直方向旋轉角（上下看）
    private float cinemachineTargetYaw;
    // 閥值
    private const float THRESHOLD = 0.01f;

    private void Start()
    {
        // 若未指定目標則尋找子物件 CameraTarget
        if (CinemachineCameraTarget == null)
        {
            Transform found = transform.Find("CameraTarget");
            if (found != null)
                CinemachineCameraTarget = found.gameObject;
            else
                Debug.LogError("找不到 CameraTarget 子物件！");
        }
        // 初始化旋轉角度
        cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        input = GetComponent<StarterAssetsInputs>();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void CameraRotation()
    {
        //如果有輸入(設定閥值避免抖動) & 相機未鎖定
        if (input != null && input.look.sqrMagnitude >= THRESHOLD && !LockCameraPosition)
        {
            cinemachineTargetYaw += input.look.x;
            cinemachineTargetPitch += -input.look.y;
        }
        //限制旋轉角度在 360 度以內。
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);
        // Cinemachine 將跟著這一目標物體的旋轉
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + CameraAngleOverride, cinemachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}
