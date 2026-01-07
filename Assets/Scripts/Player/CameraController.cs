using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// 攝影機控制器 - 處理攝影機旋轉和縮放
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("組件引用")]
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private GameObject cinemachineCameraTarget;

    [Header("攝影機設置")]
    [SerializeField] private float cameraAngleOverride = 0.0f;
    [SerializeField] private bool lockCameraPosition = false;

    [Header("視角限制")]
    [SerializeField] private float topClamp = 70.0f;
    [SerializeField] private float bottomClamp = -30.0f;

    [Header("縮放參數")]
    [SerializeField] private float minFOV = 40.0f;
    [SerializeField] private float maxFOV = 70.0f;
    [SerializeField] private float zoomSmoothTime = 0.1f;
    [SerializeField] private float zoomSpeed = 2.0f;

    // 內部狀態
    private StarterAssetsInputs input;
    private float cinemachineTargetPitch;
    private float cinemachineTargetYaw;
    private float targetFOV = 60.0f;
    private float zoomVelocity = 0.0f;
    private bool isInitialized = false;

    private const float INPUT_THRESHOLD = 0.01f;

    private void Start()
    {
        StartCoroutine(WaitForCameraAndInit());
    }

    private System.Collections.IEnumerator WaitForCameraAndInit()
    {
        float timeout = 3f;
        float timer = 0f;

        // 等待所有組件就緒
        while ((!virtualCamera || !cinemachineCameraTarget) && timer < timeout)
        {
            if (!virtualCamera)
            {
                virtualCamera = FindAnyObjectByType<CinemachineCamera>();
            }

            if (!cinemachineCameraTarget)
            {
                Transform found = transform.Find("CameraTarget");
                if (found != null)
                {
                    cinemachineCameraTarget = found.gameObject;
                }
            }

            if (virtualCamera && cinemachineCameraTarget)
                break;

            yield return null;
            timer += Time.unscaledDeltaTime;
        }

        // 驗證組件
        if (!virtualCamera)
        {
            Debug.LogError("[CameraController] 找不到 CinemachineCamera 元件！");
            yield break;
        }

        if (!cinemachineCameraTarget)
        {
            Debug.LogError("[CameraController] 找不到 CameraTarget 子物件！");
            yield break;
        }

        // 初始化
        InitializeCamera();
    }

    private void InitializeCamera()
    {
        // 設置初始旋轉角度
        cinemachineTargetYaw = cinemachineCameraTarget.transform.rotation.eulerAngles.y;
        cinemachineTargetPitch = cinemachineCameraTarget.transform.rotation.eulerAngles.x;

        // 取得輸入組件
        if (!input)
        {
            input = GetComponent<StarterAssetsInputs>();
        }

        // 設置初始 FOV
        if (virtualCamera)
        {
            targetFOV = virtualCamera.Lens.FieldOfView;
        }

        isInitialized = true;
        Debug.Log("[CameraController] 攝影機初始化完成");
    }

    private void LateUpdate()
    {
        if (!isInitialized || !cinemachineCameraTarget) return;

        CameraRotation();
        CameraZoom();
    }

    private void CameraRotation()
    {
        // 檢查是否有輸入且相機未鎖定
        if (input && input.look.sqrMagnitude >= INPUT_THRESHOLD && !lockCameraPosition)
        {
            cinemachineTargetYaw += input.look.x;
            cinemachineTargetPitch += -input.look.y;
        }

        // 限制旋轉角度
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

        // 應用旋轉
        if (cinemachineCameraTarget)
        {
            cinemachineCameraTarget.transform.rotation = Quaternion.Euler(
                cinemachineTargetPitch + cameraAngleOverride,
                cinemachineTargetYaw,
                0.0f
            );
        }
    }

    private void CameraZoom()
    {
        if (!virtualCamera) return;

        // 處理縮放輸入
        if (input && Mathf.Abs(input.zoom) > INPUT_THRESHOLD)
        {
            targetFOV -= input.zoom * zoomSpeed;
            targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
        }

        // 平滑應用 FOV
        virtualCamera.Lens.FieldOfView = Mathf.SmoothDamp(
            virtualCamera.Lens.FieldOfView,
            targetFOV,
            ref zoomVelocity,
            zoomSmoothTime
        );
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    /// <summary>
    /// 重置攝影機視角
    /// </summary>
    public void ResetCameraRotation()
    {
        if (!cinemachineCameraTarget) return;

        cinemachineTargetYaw = 0f;
        cinemachineTargetPitch = 0f;
        cinemachineCameraTarget.transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// 重置縮放
    /// </summary>
    public void ResetZoom()
    {
        targetFOV = 60.0f;
    }

    /// <summary>
    /// 設置相機鎖定狀態
    /// </summary>
    public void SetCameraLock(bool locked)
    {
        lockCameraPosition = locked;
    }

    /// <summary>
    /// 檢查是否已初始化
    /// </summary>
    public bool IsInitialized()
    {
        return isInitialized;
    }
}
