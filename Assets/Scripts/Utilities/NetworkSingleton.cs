using Unity.Netcode;
using UnityEngine;

/// <summary>
/// NetworkSingleton 基類 - 用於 NetworkBehaviour 的 Singleton
/// 場景切換時會被銷毀，適用於場景特定的網路管理器
/// </summary>
/// <typeparam name="T">繼承此類的具體類型</typeparam>
public class NetworkSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning($"[NetworkSingleton] Instance '{typeof(T)}' 已被銷毀（應用程式正在關閉）。");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        Debug.LogError($"[NetworkSingleton] 場景中未找到 '{typeof(T)}' 的實例！");
                    }
                }

                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"[NetworkSingleton] 場景中檢測到多個 '{typeof(T)}' 實例，正在銷毀重複實例。");
            Destroy(gameObject);
        }
    }

    protected virtual new void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }
}
