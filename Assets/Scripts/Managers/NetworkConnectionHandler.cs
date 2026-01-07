using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 網路連線處理器 - 處理網路連線狀態、斷線重連等邏輯
/// </summary>
public class NetworkConnectionHandler : PersistentSingleton<NetworkConnectionHandler>
{
    [Header("連線設定")]
    [SerializeField] private float connectionTimeout = 30f;
    [SerializeField] private bool autoReconnectOnDisconnect = false;
    [SerializeField] private int maxReconnectAttempts = 3;

    // 連線事件
    public event Action OnConnectionEstablished;
    public event Action OnConnectionLost;
    public event Action<string> OnConnectionError;

    private bool isConnecting = false;
    private int reconnectAttempts = 0;
    private Coroutine connectionTimeoutCoroutine;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        SubscribeToNetworkEvents();
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        UnsubscribeFromNetworkEvents();
    }

    private void SubscribeToNetworkEvents()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("[NetworkConnectionHandler] NetworkManager 尚未初始化，等待初始化後訂閱...");
            StartCoroutine(WaitForNetworkManagerAndSubscribe());
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;
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
            NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;
            Debug.Log("[NetworkConnectionHandler] NetworkManager 初始化完成，已訂閱網路事件");
        }
        else
        {
            Debug.LogError("[NetworkConnectionHandler] NetworkManager 初始化超時！");
        }
    }

    private void UnsubscribeFromNetworkEvents()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnTransportFailure -= OnTransportFailure;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[NetworkConnectionHandler] 客戶端已連線: {clientId}");

        // 如果是本地客戶端
        if (NetworkManager.Singleton != null && clientId == NetworkManager.Singleton.LocalClientId)
        {
            isConnecting = false;
            reconnectAttempts = 0;

            if (connectionTimeoutCoroutine != null)
            {
                StopCoroutine(connectionTimeoutCoroutine);
                connectionTimeoutCoroutine = null;
            }

            OnConnectionEstablished?.Invoke();
            Debug.Log("[NetworkConnectionHandler] 本地客戶端連線成功");
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"[NetworkConnectionHandler] 客戶端已斷線: {clientId}");

        // 如果是本地客戶端斷線
        if (NetworkManager.Singleton != null && clientId == NetworkManager.Singleton.LocalClientId)
        {
            HandleLocalDisconnection();
        }
    }

    private void OnTransportFailure()
    {
        Debug.LogError("[NetworkConnectionHandler] 傳輸層失敗");
        OnConnectionError?.Invoke("Transport failure");
        HandleConnectionError();
    }

    /// <summary>
    /// 處理本地客戶端斷線
    /// </summary>
    private void HandleLocalDisconnection()
    {
        Debug.LogWarning("[NetworkConnectionHandler] 本地客戶端斷線");

        isConnecting = false;

        if (connectionTimeoutCoroutine != null)
        {
            StopCoroutine(connectionTimeoutCoroutine);
            connectionTimeoutCoroutine = null;
        }

        OnConnectionLost?.Invoke();

        // 自動重連邏輯
        if (autoReconnectOnDisconnect && reconnectAttempts < maxReconnectAttempts)
        {
            StartCoroutine(AttemptReconnect());
        }
        else
        {
            // 返回大廳
            ReturnToLobby();
        }
    }

    /// <summary>
    /// 處理連線錯誤
    /// </summary>
    private void HandleConnectionError()
    {
        Debug.LogError("[NetworkConnectionHandler] 連線錯誤，返回大廳");
        ReturnToLobby();
    }

    /// <summary>
    /// 嘗試重新連線
    /// </summary>
    private IEnumerator AttemptReconnect()
    {
        reconnectAttempts++;
        Debug.Log($"[NetworkConnectionHandler] 嘗試重新連線 ({reconnectAttempts}/{maxReconnectAttempts})");

        yield return new WaitForSeconds(2f);

        // 這裡可以實作重連邏輯
        // 目前先返回大廳
        if (reconnectAttempts >= maxReconnectAttempts)
        {
            Debug.LogWarning("[NetworkConnectionHandler] 重連失敗，返回大廳");
            ReturnToLobby();
        }
    }

    /// <summary>
    /// 開始連線超時檢查
    /// </summary>
    public void StartConnectionTimeout()
    {
        if (connectionTimeoutCoroutine != null)
        {
            StopCoroutine(connectionTimeoutCoroutine);
        }

        connectionTimeoutCoroutine = StartCoroutine(ConnectionTimeoutCoroutine());
    }

    private IEnumerator ConnectionTimeoutCoroutine()
    {
        isConnecting = true;
        float elapsedTime = 0f;

        while (elapsedTime < connectionTimeout && isConnecting)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (isConnecting)
        {
            Debug.LogError("[NetworkConnectionHandler] 連線超時");
            OnConnectionError?.Invoke("Connection timeout");
            HandleConnectionError();
        }

        connectionTimeoutCoroutine = null;
    }

    /// <summary>
    /// 返回大廳
    /// </summary>
    private void ReturnToLobby()
    {
        reconnectAttempts = 0;

        if (SceneLifecycleManager.Instance != null)
        {
            SceneLifecycleManager.Instance.RequestReturnToLobby();
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.EndGameAndReturnToLobby();
        }
    }

    /// <summary>
    /// 手動斷開連線
    /// </summary>
    public void Disconnect()
    {
        Debug.Log("[NetworkConnectionHandler] 手動斷開連線");

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
    }

    /// <summary>
    /// 檢查是否已連線
    /// </summary>
    public bool IsConnected()
    {
        return NetworkManager.Singleton != null &&
               (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer);
    }

    /// <summary>
    /// 重置重連次數
    /// </summary>
    public void ResetReconnectAttempts()
    {
        reconnectAttempts = 0;
    }
}
