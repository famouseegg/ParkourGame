using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 遊戲管理器 - 持久化跨場景，負責遊戲生命週期管理
/// </summary>
public class GameManager : PersistentSingleton<GameManager>
{
    [Header("遊戲狀態")]
    [SerializeField] private bool isGameStarted = false;

    // 遊戲事件
    public event Action<bool> OnGameStarted; // 參數: isHost
    public event Action OnGameEnded;
    public event Action OnReturnToLobby;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        // 延遲到 Start 訂閱，確保 NetworkManager 已初始化
        SubscribeToNetworkEvents();
    }

    private void SubscribeToNetworkEvents()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("[GameManager] NetworkManager 尚未初始化，等待初始化後訂閱...");
            StartCoroutine(WaitForNetworkManagerAndSubscribe());
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private System.Collections.IEnumerator WaitForNetworkManagerAndSubscribe()
    {
        // 等待 NetworkManager 初始化（最多等待 10 秒）
        float timeout = 10f;
        float elapsed = 0f;

        while (NetworkManager.Singleton == null && elapsed < timeout)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            Debug.Log("[GameManager] NetworkManager 初始化完成，已訂閱網路事件");
        }
        else
        {
            Debug.LogError("[GameManager] NetworkManager 初始化超時！");
        }
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        UnsubscribeFromNetworkEvents();
    }

    private void UnsubscribeFromNetworkEvents()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[GameManager] 客戶端已連線: {clientId}");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"[GameManager] 客戶端已斷線: {clientId}");

        // 如果是本地客戶端斷線，返回大廳
        if (NetworkManager.Singleton != null && clientId == NetworkManager.Singleton.LocalClientId)
        {
            HandleLocalDisconnect();
        }
    }

    /// <summary>
    /// 啟動遊戲 - 初始化網路連線
    /// </summary>
    public void StartGame(bool isHost)
    {
        if (isGameStarted)
        {
            Debug.LogWarning("[GameManager] 遊戲已經啟動！");
            return;
        }

        var nm = NetworkManager.Singleton;
        if (nm == null)
        {
            Debug.LogError("[GameManager] NetworkManager 不存在！");
            return;
        }

        // 驗證 Transport
        if (nm.NetworkConfig.NetworkTransport == null)
        {
            Debug.LogError("[GameManager] NetworkTransport 未設置！");
            return;
        }

        if (!nm.IsClient && !nm.IsServer)
        {
            if (isHost)
            {
                Debug.Log("[GameManager] 啟動 Host 模式...");
                nm.StartHost();
            }
            else
            {
                Debug.Log("[GameManager] 啟動 Client 模式...");

                // Client 啟動前的最後檢查
                var transport = nm.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
                if (transport == null)
                {
                    Debug.LogError("[GameManager] UnityTransport 組件不存在！");
                    return;
                }

                Debug.Log($"[GameManager] Transport 狀態檢查通過，開始連線...");
                nm.StartClient();
            }

            isGameStarted = true;
            OnGameStarted?.Invoke(isHost);

            // NetworkManager 啟動後，通知 NetworkSceneManager 訂閱事件
            if (NetworkSceneManager.Instance != null)
            {
                NetworkSceneManager.Instance.SubscribeToNetworkEvents();
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] 網路已啟動，忽略重複請求");
        }
    }

    /// <summary>
    /// 結束遊戲並返回大廳
    /// </summary>
    public void EndGameAndReturnToLobby()
    {
        Debug.Log("[GameManager] 結束遊戲並返回大廳");

        isGameStarted = false;
        OnGameEnded?.Invoke();

        // 通過場景管理器返回大廳
        NetworkSceneManager.Instance.LoadLobbyScene();

        OnReturnToLobby?.Invoke();
    }

    /// <summary>
    /// 處理本地玩家斷線
    /// </summary>
    private void HandleLocalDisconnect()
    {
        Debug.LogWarning("[GameManager] 本地玩家已斷線，返回大廳...");
        EndGameAndReturnToLobby();
    }

    /// <summary>
    /// 檢查遊戲是否已啟動
    /// </summary>
    public bool IsGameStarted()
    {
        return isGameStarted;
    }

    /// <summary>
    /// 重置遊戲狀態（用於返回大廳後）
    /// </summary>
    public void ResetGameState()
    {
        isGameStarted = false;
    }
}

