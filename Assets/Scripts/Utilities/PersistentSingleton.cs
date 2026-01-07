using UnityEngine;

/// <summary>
/// 持久化 Singleton 基類 - 場景切換時不會被銷毀
/// 適用於需要跨場景存在的全局管理器（如 LobbyManager、RelayManager）
/// </summary>
/// <typeparam name="T">繼承此類的具體類型</typeparam>
public class PersistentSingleton<T> : MonoBehaviour where T : MonoBehaviour
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
                Debug.LogWarning($"[PersistentSingleton] Instance '{typeof(T)}' 已被銷毀（應用程式正在關閉）。");
                return null;
            }

            // Double-Check Locking：第一次檢查（無鎖）
            if (_instance == null)
            {
                lock (_lock)
                {
                    // 第二次檢查（有鎖）
                    if (_instance == null)
                    {
                        _instance = FindFirstObjectByType<T>();

                        if (_instance == null)
                        {
                            Debug.LogError($"[PersistentSingleton] 場景中未找到 '{typeof(T)}' 的實例！請確保在場景中添加該組件。");
                        }
                    }
                }
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"[PersistentSingleton] 檢測到重複的 '{typeof(T)}' 實例，正在銷毀。");
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }
}
