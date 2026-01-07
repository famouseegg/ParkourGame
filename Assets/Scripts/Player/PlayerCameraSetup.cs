using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 玩家攝影機設置 - 負責綁定 Cinemachine 相機到玩家
/// 已整合場景生命週期管理
/// </summary>
public class PlayerCameraSetup : NetworkBehaviour
{
    [SerializeField] private Transform cameraTarget;

    private CinemachineCamera boundCamera;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        // 訂閱場景生命週期事件
        if (SceneLifecycleManager.Instance != null)
        {
            SceneLifecycleManager.Instance.OnScenePostLoad += OnSceneLoadComplete;
        }

        BindCamera();
    }

    public override void OnNetworkDespawn()
    {
        // 取消訂閱
        if (SceneLifecycleManager.Instance != null)
        {
            SceneLifecycleManager.Instance.OnScenePostLoad -= OnSceneLoadComplete;
        }

        // 解除相機綁定
        UnbindCamera();
    }

    private void OnSceneLoadComplete(string sceneName)
    {
        if (!IsOwner) return;

        Debug.Log($"[PlayerCameraSetup] 場景載入完成: {sceneName}，重新綁定相機");
        BindCamera();
    }

    private void BindCamera()
    {
        if (!IsOwner) return;
        StartCoroutine(WaitForCameraAndBind());
    }

    private void UnbindCamera()
    {
        if (boundCamera != null)
        {
            boundCamera.Follow = null;
            boundCamera = null;
            Debug.Log("[PlayerCameraSetup] 相機已解除綁定");
        }
    }

    private System.Collections.IEnumerator WaitForCameraAndBind()
    {
        CinemachineCamera cam = null;
        float timeout = 3f;
        float timer = 0f;

        while (cam == null && timer < timeout)
        {
            cam = FindAnyObjectByType<CinemachineCamera>();

            if (cam != null)
            {
                // 如果已經綁定到其他相機，先解除
                if (boundCamera != null && boundCamera != cam)
                {
                    boundCamera.Follow = null;
                }

                boundCamera = cam;
                cam.Follow = cameraTarget;
                Debug.Log("[PlayerCameraSetup] 成功綁定 CinemachineCamera");
                yield break;
            }

            yield return null;
            timer += Time.unscaledDeltaTime;
        }

        Debug.LogWarning("[PlayerCameraSetup] 等待超時，找不到 CinemachineCamera");
    }

    /// <summary>
    /// 手動重新綁定相機（調試用）
    /// </summary>
    public void RebindCamera()
    {
        if (!IsOwner) return;

        UnbindCamera();
        BindCamera();
    }
}
