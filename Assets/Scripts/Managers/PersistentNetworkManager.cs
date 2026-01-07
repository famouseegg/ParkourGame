using UnityEngine;
using Unity.Netcode;

/// <summary>
/// 持久化 NetworkManager - 確保 NetworkManager 跨場景不被銷毀
/// 這是 Unity Netcode 的最佳實踐
/// </summary>
[RequireComponent(typeof(NetworkManager))]
[DefaultExecutionOrder(-100)] // 確保在其他腳本之前執行
public class PersistentNetworkManager : MonoBehaviour
{
    private static PersistentNetworkManager instance;

    private void Awake()
    {
        // 確保只有一個實例
        if (instance != null && instance != this)
        {
            Debug.LogWarning("[PersistentNetworkManager] 檢測到重複的 NetworkManager，銷毀新的實例");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // 驗證必要組件
        ValidateComponents();
    }

    private void ValidateComponents()
    {
        var networkManager = GetComponent<NetworkManager>();
        if (networkManager == null)
        {
            Debug.LogError("[PersistentNetworkManager] 找不到 NetworkManager 組件！");
            return;
        }

        if (networkManager.NetworkConfig == null)
        {
            Debug.LogError("[PersistentNetworkManager] NetworkManager 的 NetworkConfig 為空！");
            return;
        }

        if (networkManager.NetworkConfig.NetworkTransport == null)
        {
            Debug.LogError("[PersistentNetworkManager] NetworkManager 沒有配置 Network Transport！");
            Debug.LogError("[PersistentNetworkManager] 請在 Inspector 中添加 Unity Transport 組件");
            return;
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
