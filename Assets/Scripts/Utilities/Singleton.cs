using UnityEngine;

/// <summary>
/// 通用 Singleton 基類 - 場景切換時會被銷毀
/// 適用於場景特定的管理器
/// </summary>
/// <typeparam name="T">繼承此類的具體類型</typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
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
                Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' 已被銷毀（應用程式正在關閉）。");
                return null;
            }

            // Double-Check Locking ：第一次檢查（無鎖）
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
                            Debug.LogError($"[Singleton] 場景中未找到 '{typeof(T)}' 的實例！");
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
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"[Singleton] 場景中檢測到多個 '{typeof(T)}' 實例，正在銷毀重複實例。");
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy()
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
