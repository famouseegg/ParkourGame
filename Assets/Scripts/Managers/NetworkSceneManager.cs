using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 統一的網路場景管理器
/// 負責處理多人遊戲中的場景切換，包含完整的生命週期管理
/// </summary>
public class NetworkSceneManager : PersistentSingleton<NetworkSceneManager>
{
    [Header("場景名稱配置")]
    [SerializeField] private string lobbySceneName = "LobbyScene";

    // 場景切換事件
    public event Action OnSceneLoadStarted;
    public event Action<string> OnSceneLoadComplete;
    public event Action OnSceneUnloadStarted;

    private bool _isLoadingScene = false;
    private string _currentSceneName;

    protected override void Awake()
    {
        base.Awake();
        _currentSceneName = SceneManager.GetActiveScene().name;
    }

    private void Start()
    {
        // 檢查當前是否在大廳場景
        bool isInLobbyScene = _currentSceneName == lobbySceneName;

        // 遊戲場景中，嘗試訂閱事件
        if (!isInLobbyScene)
            StartCoroutine(WaitForNetworkManagerAndSubscribe());
    }

    private IEnumerator WaitForNetworkManagerAndSubscribe()
    {
        // 等待 NetworkManager 初始化
        float timeout = 5f;
        float elapsed = 0f;

        while ((NetworkManager.Singleton == null || NetworkManager.Singleton.SceneManager == null) && elapsed < timeout)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            SubscribeToNetworkEvents();
        }
        else
        {
            Debug.LogError("[NetworkSceneManager] 在遊戲場景中找不到 NetworkManager！這不應該發生。");
        }
    }

    /// <summary>
    /// 訂閱網路場景事件（可以在遊戲開始時手動調用）
    /// </summary>
    public void SubscribeToNetworkEvents()
    {
        if (NetworkManager.Singleton == null || NetworkManager.Singleton.SceneManager == null)
        {
            Debug.LogWarning("[NetworkSceneManager] NetworkManager 尚未初始化，無法訂閱事件");
            return;
        }

        // 避免重複訂閱
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnNetworkSceneLoadCompleted;
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnNetworkLoadComplete;

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnNetworkSceneLoadCompleted;
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnNetworkLoadComplete;
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        // 取消訂閱事件
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnNetworkSceneLoadCompleted;
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnNetworkLoadComplete;
        }
    }

    /// <summary>
    /// 載入遊戲場景（僅 Server/Host 可呼叫）
    /// </summary>
    public void LoadGameScene(string sceneName)
    {
        if (_isLoadingScene)
        {
            Debug.LogWarning($"[NetworkSceneManager] 場景切換進行中，忽略請求: {sceneName}");
            return;
        }

        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError("[NetworkSceneManager] 只有 Server/Host 可以載入場景！");
            return;
        }

        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    /// <summary>
    /// 返回大廳場景
    /// </summary>
    public void LoadLobbyScene()
    {
        if (_isLoadingScene)
        {
            Debug.LogWarning("[NetworkSceneManager] 場景切換進行中，忽略返回大廳請求");
            return;
        }

        StartCoroutine(UnloadNetworkAndLoadLobby());
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        _isLoadingScene = true;
        OnSceneLoadStarted?.Invoke();

        Debug.Log($"[NetworkSceneManager] 開始載入場景: {sceneName}");

        // 使用 Netcode 的場景管理系統
        var status = NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogError($"[NetworkSceneManager] 場景載入失敗: {sceneName}, Status: {status}");
            _isLoadingScene = false;
            yield break;
        }

        // 等待場景載入完成（添加超時機制）
        float timeout = 60f; // 增加超時時間到 60 秒
        float elapsed = 0f;

        while (_isLoadingScene && elapsed < timeout)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }

        if (_isLoadingScene)
        {
            Debug.LogError($"[NetworkSceneManager] 場景載入超時！場景: {sceneName}，已等待 {timeout} 秒");
            Debug.LogError("[NetworkSceneManager] 可能原因：OnLoadEventCompleted 事件未被觸發");
            _isLoadingScene = false;
            // 可以在這裡觸發錯誤處理，例如返回大廳
        }
        else
        {
            Debug.Log($"[NetworkSceneManager] 場景載入協程完成: {sceneName}");
        }
    }

    private IEnumerator UnloadNetworkAndLoadLobby()
    {
        _isLoadingScene = true;
        OnSceneUnloadStarted?.Invoke();

        Debug.Log("[NetworkSceneManager] 準備返回大廳，關閉網路連線...");

        // 關閉網路連線
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.Shutdown();
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.Shutdown();
            }
        }

        // 等待網路完全關閉
        yield return new WaitForSeconds(0.5f);

        // 載入大廳場景
        Debug.Log($"[NetworkSceneManager] 載入大廳場景: {lobbySceneName}");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(lobbySceneName, LoadSceneMode.Single);

        yield return asyncLoad;

        _currentSceneName = lobbySceneName;
        _isLoadingScene = false;

        OnSceneLoadComplete?.Invoke(lobbySceneName);
        Debug.Log($"[NetworkSceneManager] 大廳場景載入完成");
    }

    /// <summary>
    /// Netcode 場景載入完成事件
    /// </summary>
    private void OnNetworkSceneLoadCompleted(string sceneName, LoadSceneMode loadSceneMode, System.Collections.Generic.List<ulong> clientsCompleted, System.Collections.Generic.List<ulong> clientsTimedOut)
    {
        Debug.Log($"[NetworkSceneManager] 網路場景載入完成: {sceneName}");
        Debug.Log($"[NetworkSceneManager] 完成的客戶端數量: {clientsCompleted.Count}, 超時客戶端: {clientsTimedOut.Count}");

        _currentSceneName = sceneName;
        _isLoadingScene = false;

        OnSceneLoadComplete?.Invoke(sceneName);
    }

    /// <summary>
    /// 本地客戶端場景載入完成
    /// </summary>
    private void OnNetworkLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        Debug.Log($"[NetworkSceneManager] 客戶端 {clientId} 載入場景完成: {sceneName}");
    }

    /// <summary>
    /// 獲取當前場景名稱
    /// </summary>
    public string GetCurrentSceneName()
    {
        return _currentSceneName;
    }

    /// <summary>
    /// 檢查是否在大廳場景
    /// </summary>
    public bool IsInLobby()
    {
        return _currentSceneName == lobbySceneName;
    }

    /// <summary>
    /// 檢查是否正在載入場景
    /// </summary>
    public bool IsLoadingScene()
    {
        return _isLoadingScene;
    }
}
