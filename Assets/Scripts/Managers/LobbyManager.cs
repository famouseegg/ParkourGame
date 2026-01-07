// LobbyManager.cs
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using System;

/// <summary>
/// 大廳管理器 - 持久化跨場景，負責 Unity Lobby Service 的所有操作
/// </summary>
public class LobbyManager : PersistentSingleton<LobbyManager>
{
    private Lobby joinedLobby;
    private Lobby hostLobby;
    private float heartbeatTimer;
    private float updateLobbyDataTimer;
    private float updateListTimer;
    private string playerName;
    private bool hasJoinedRelay;
    private bool isInitialized;

    // 大廳事件
    public event EventHandler<OnListLobbiesArgs> OnListLobbies;
    public event EventHandler<OnPrintPlayerArgs> OnPrintPlayers;
    public event EventHandler OnLobbyJoined;
    public event EventHandler OnLobbyLeft;

    public class OnListLobbiesArgs : EventArgs
    {
        public List<Lobby> LobbyList;
    }

    public class OnPrintPlayerArgs : EventArgs
    {
        public Lobby lobby;
    }

    protected override void Awake()
    {
        base.Awake();
        isInitialized = false;
    }

    private async void Start()
    {
        if (isInitialized) return;

        try
        {
            // 初始化 Unity Services
            await UnityServices.InitializeAsync();

            // 使用匿名方式登入 Unity Authentication 服務
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            playerName = $"Player {UnityEngine.Random.Range(10, 99)}";
            hasJoinedRelay = false;
            isInitialized = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[LobbyManager] 初始化失敗: {e}");
        }
    }

    private void Update()
    {
        if (!isInitialized) return;

        HandleHeartbeat();
        HandleLobbyPollForUpdate();

        // 只在大廳場景時更新 UI
        if (IsInLobbyScene())
        {
            UpdateLobbyList();
        }
    }

    /// <summary>
    /// 檢查是否在大廳場景
    /// </summary>
    private bool IsInLobbyScene()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        // 根據您的場景命名調整（例如 "Lobby", "MainMenu", "LobbyScene" 等）
        return currentScene.Contains("Lobby") || currentScene.Contains("Menu") || currentScene == "SampleScene";
    }

    private async void HandleLobbyPollForUpdate()
    {
        if (joinedLobby != null)
        {
            updateLobbyDataTimer -= Time.deltaTime;
            if (updateLobbyDataTimer <= 0f)
            {
                float updateTimeMax = 5f;
                updateLobbyDataTimer = updateTimeMax;

                try
                {
                    Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                    joinedLobby = lobby;
                    PrintPlayers(joinedLobby);
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogWarning($"[LobbyManager] 更新大廳資料失敗: {e}");
                }
            }

            if (!IsHost())
            {
                if (joinedLobby.Data != null && joinedLobby.Data.ContainsKey("RelayJoinCode"))
                {
                    if (hasJoinedRelay)
                        return;

                    hasJoinedRelay = true;

                    string relayJoinCode = joinedLobby.Data["RelayJoinCode"].Value;

                    Debug.Log($"[LobbyManager] 取得 Relay JoinCode: {relayJoinCode}");

                    try
                    {
                        await RelayManager.Instance.JoinRelay(relayJoinCode);

                        // 等待 Transport 設置完成
                        await System.Threading.Tasks.Task.Delay(300);

                        // 檢查 NetworkManager 和 Transport 是否就緒
                        if (NetworkManager.Singleton == null)
                        {
                            Debug.LogError("[LobbyManager] NetworkManager 不存在！");
                            hasJoinedRelay = false;
                            return;
                        }

                        var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
                        if (transport == null)
                        {
                            Debug.LogError("[LobbyManager] UnityTransport 組件不存在！");
                            hasJoinedRelay = false;
                            return;
                        }

                        Debug.Log("[LobbyManager] NetworkManager 和 Transport 已就緒，啟動 Client...");
                        GameManager.Instance.StartGame(false);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[LobbyManager] 加入遊戲失敗: {e}");
                        hasJoinedRelay = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 房間開啟後一段時間就會關閉，因此需要定期發送心跳包以維持房間存活
    /// </summary>
    private async void HandleHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0f)
            {
                float heartTimerMax = 15f;
                heartbeatTimer = heartTimerMax;

                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogWarning($"[LobbyManager] 心跳包發送失敗: {e}");
                }
            }
        }
    }

    private void UpdateLobbyList()
    {
        if (LobbyUIController.Instance != null && LobbyUIController.Instance.Getstate() == LobbyUIController.State.LobbyList)
        {
            updateListTimer -= Time.deltaTime;
            if (updateListTimer <= 0f)
            {
                float updateListTimerMax = 5f;
                updateListTimer = updateListTimerMax;
                ListLobbies();
            }
        }
    }

    private async void CreatLobby(int maxPlayers, string lobbyName)
    {
        try
        {
            if (string.IsNullOrEmpty(lobbyName))
                lobbyName = "Lobby";

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer()
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            joinedLobby = lobby;
            hostLobby = lobby;

            OnLobbyJoined?.Invoke(this, EventArgs.Empty);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"[LobbyManager] 建立大廳失敗: {e}");
        }
    }

    private async Task CreatRelayAndUpdateLobbyData()
    {
        string relayJoinCode = await RelayManager.Instance.CreatRelay();

        Debug.Log($"[LobbyManager] 取得 Relay Join Code: {relayJoinCode}");

        // 更新 Lobby Data
        await LobbyService.Instance.UpdateLobbyAsync(
            joinedLobby.Id,
            new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {
                        "RelayJoinCode",
                        new DataObject(
                            DataObject.VisibilityOptions.Public,
                            relayJoinCode
                        )
                    }
                }
            }
        );
    }

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>(){
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>()
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            OnListLobbies?.Invoke(this, new OnListLobbiesArgs { LobbyList = queryResponse.Results });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning($"[LobbyManager] 查詢大廳失敗: {e}");
        }
    }

    private async void JoinLobbyById(Lobby lobby)
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, joinLobbyByIdOptions);

            Debug.Log($"[LobbyManager] 成功加入大廳 ID: {lobby.Id}");

            OnLobbyJoined?.Invoke(this, EventArgs.Empty);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"[LobbyManager] 加入大廳失敗: {e}");
        }
    }

    private void PrintPlayers(Lobby lobby)
    {
        OnPrintPlayers?.Invoke(this, new OnPrintPlayerArgs { lobby = lobby });
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {
                    "PlayerName",
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)
                }
            }
        };
    }

    private async void LeaveLobby()
    {
        try
        {
            if (joinedLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                Debug.Log("[LobbyManager] 已離開大廳");
            }

            hostLobby = null;
            joinedLobby = null;
            hasJoinedRelay = false;

            OnLobbyLeft?.Invoke(this, EventArgs.Empty);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning($"[LobbyManager] 離開大廳失敗: {e}");
        }
    }

    public bool IsHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    // === 公開 API ===

    public void CreatLobbyButtonClick(int maxPlayers, string lobbyName, string playerName)
    {
        if (!string.IsNullOrEmpty(playerName))
            this.playerName = playerName;
        CreatLobby(maxPlayers, lobbyName);
    }

    public void LeaveLobbyButtOnClick()
    {
        LeaveLobby();
        ListLobbies();
    }

    public void JoinLobbyButtonOnClick(Lobby lobby, string playerName)
    {
        if (!string.IsNullOrEmpty(playerName))
            this.playerName = playerName;
        JoinLobbyById(lobby);
    }

    public void ListLobbyButtonOnClick()
    {
        ListLobbies();
    }

    /// <summary>
    /// 開始遊戲並切換場景
    /// </summary>
    public async void OnGameStartWithScene(string sceneName)
    {
        await CreatRelayAndUpdateLobbyData();
        GameManager.Instance.StartGame(true); // 先啟動 Host

        // 等待網路初始化
        await Task.Delay(500);

        // 使用新的場景管理器
        NetworkSceneManager.Instance.LoadGameScene(sceneName);
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public bool IsInLobby()
    {
        return joinedLobby != null;
    }

    /// <summary>
    /// 清理大廳狀態（返回大廳場景時呼叫）
    /// </summary>
    public void CleanupLobbyState()
    {
        hasJoinedRelay = false;
    }
}
