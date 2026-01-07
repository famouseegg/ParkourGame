using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

/// <summary>
/// 場景生命週期管理器 - 統一處理場景切換時的清理和初始化邏輯
/// </summary>
public class SceneLifecycleManager : PersistentSingleton<SceneLifecycleManager>
{
    // 場景生命週期事件
    // public event Action<string> OnScenePreLoad;      // 場景載入前（保留供未來使用）
    public event Action<string> OnScenePostLoad;     // 場景載入後
    // public event Action<string> OnScenePreUnload;    // 場景卸載前（保留供未來使用）
    public event Action OnReturnToLobbyRequested;    // 請求返回大廳

    [Header("場景名稱配置")]
    [SerializeField] private string lobbySceneName = "LobbyScene";

    protected override void Awake()
    {
        base.Awake();

        // 訂閱場景切換事件
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        // 取消訂閱
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void Start()
    {
        // 訂閱網路場景管理器事件
        if (NetworkSceneManager.Instance != null)
        {
            NetworkSceneManager.Instance.OnSceneLoadStarted += HandleSceneLoadStarted;
            NetworkSceneManager.Instance.OnSceneLoadComplete += HandleSceneLoadComplete;
            NetworkSceneManager.Instance.OnSceneUnloadStarted += HandleSceneUnloadStarted;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[SceneLifecycleManager] 場景已載入: {scene.name}");

        // 場景載入後的初始化
        if (scene.name == lobbySceneName)
        {
            OnReturnedToLobby();
        }
        else
        {
            OnGameSceneLoaded(scene.name);
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"[SceneLifecycleManager] 場景已卸載: {scene.name}");
    }

    private void HandleSceneLoadStarted()
    {
        Debug.Log("[SceneLifecycleManager] 開始載入場景");
    }

    private void HandleSceneLoadComplete(string sceneName)
    {
        Debug.Log($"[SceneLifecycleManager] 場景載入完成: {sceneName}");
        OnScenePostLoad?.Invoke(sceneName);
    }

    private void HandleSceneUnloadStarted()
    {
        Debug.Log("[SceneLifecycleManager] 開始卸載場景");

        // 清理遊戲狀態
        CleanupGameState();
    }

    /// <summary>
    /// 返回大廳時的清理邏輯
    /// </summary>
    private void OnReturnedToLobby()
    {
        // 清理所有管理器狀態
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.CleanupLobbyState();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGameState();
        }

        OnScenePostLoad?.Invoke(lobbySceneName);
    }

    /// <summary>
    /// 遊戲場景載入時的初始化
    /// </summary>
    private void OnGameSceneLoaded(string sceneName)
    {
        Debug.Log($"[SceneLifecycleManager] 遊戲場景已載入: {sceneName}");
    }

    /// <summary>
    /// 清理遊戲狀態（場景切換前）
    /// </summary>
    private void CleanupGameState()
    {
        Debug.Log("[SceneLifecycleManager] 清理遊戲狀態...");

        // 這裡可以添加更多清理邏輯
        // 例如：清除臨時數據、停止協程、釋放資源等
    }

    /// <summary>
    /// 請求返回大廳
    /// </summary>
    public void RequestReturnToLobby()
    {
        Debug.Log("[SceneLifecycleManager] 請求返回大廳");
        OnReturnToLobbyRequested?.Invoke();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndGameAndReturnToLobby();
        }
    }

    /// <summary>
    /// 檢查當前是否在大廳
    /// </summary>
    public bool IsInLobby()
    {
        return SceneManager.GetActiveScene().name == lobbySceneName;
    }

    /// <summary>
    /// 獲取當前場景名稱
    /// </summary>
    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
}
