// LobbyManager.cs
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Services.Relay.Models;
using System;
public class LobbyManager : MonoBehaviour
{
    private Lobby joinedLobby;
    private Lobby hostLobby;
    public static LobbyManager Instance { get; private set; }
    private float heartbeatTimer;
    private float updateLpbbyDataTimer;
    private float updateListTimer;
    private string playerName;
    
    private bool hasJoinedRelay;
    public event EventHandler<OnListLobbiesArgs> OnListLobbies;
    public class OnListLobbiesArgs : EventArgs
    {
        public List<Lobby> LobbyList;
    }
    public event EventHandler<OnPrintPlayerArgs> OnPrintPlayers;
    public class OnPrintPlayerArgs : EventArgs
    {
        public Lobby lobby;
    }
    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning("多個 LobbyTest 實例存在於場景中，僅保留一個實例。");
        }
        else
        {
            Instance = this;
        }
    }
    private async void Start()
    {
        // 初始化 Unity Services
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("玩家已登入，玩家 ID: " + AuthenticationService.Instance.PlayerId);
        };

        // 使用匿名方式登入 Unity Authentication 服務
        // 這會自動產生一個臨時的玩家帳號 ID
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        
        playerName = $"Player {UnityEngine.Random.Range(10,99)}";
        hasJoinedRelay = false;
    }

    private void Update()
    {
        HandleHeartbeat();
        HandleLobbyPollForUpdate();
        UpdateLobbyList();
    }


    
    private async void HandleLobbyPollForUpdate()
    {
        if (joinedLobby != null)
        {
            updateLpbbyDataTimer -= Time.deltaTime;
            if (updateLpbbyDataTimer <= 0f)
            {
                float updateTimreMax = 5f;
                updateLpbbyDataTimer = updateTimreMax;
                
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
                PrintPlayers(joinedLobby);
            }
            if (!IsHost())
            {
                if (joinedLobby.Data != null && joinedLobby.Data.ContainsKey("RelayJoinCode") )
                {
                    if (hasJoinedRelay)
                        return;

                    hasJoinedRelay = true;

                    string relayJoinCode = joinedLobby.Data["RelayJoinCode"].Value;

                    Debug.Log("取得 Relay JoinCode: " + relayJoinCode);

                    await RelayManager.Instance.JoinRelay(relayJoinCode);

                    GameManager.Instance.StartGame(false);
                }
            }
        }
        
    }

    // 房間開啟後一段時間就會關閉，因此需要定期發送心跳包以維持房間存活
    private async void HandleHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0f)
            {
                float haertTimerMax = 15f;
                heartbeatTimer = haertTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);        
            }
        }
    }
    private void UpdateLobbyList()
    {
        if(LobbyUIController.Instance.Getstate() == LobbyUIController.State.LobbyList)
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
    
    private async void CreatLobby(int maxPlayers,string lobbyName)
    {
        try
        {
            if(lobbyName == null || lobbyName=="")
                lobbyName = "Lobby";
            
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                // 設定私有大廳
                IsPrivate = false,         
                Player = GetPlayer()
            };

            Lobby lobby =await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            joinedLobby = lobby;
            hostLobby = lobby;
            Debug.Log("大廳已建立: " + lobby.Name + "大廳人數上限: "+ lobby.MaxPlayers + "Lobby ID: "+ lobby.Id + "Lobby Code: " + lobby.LobbyCode);

        }catch (LobbyServiceException e)
        {
            Debug.Log("建立大廳失敗: " + e);
        }
        
    }

    private async Task CreatRelayAndUpdateLobbyData()
    {
        string relayJoinCode = await RelayManager.Instance.CreatRelay();

        Debug.Log($"GetRelay Join Code: {relayJoinCode}");
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
            // 設定查詢選項
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                //最多回傳 25 個 Lobby
                Count = 25,
                // 只查詢「可用空位 > 0」的大廳
                Filters = new List<QueryFilter>(){
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                // false 遞增 created 欄位 (也就是依照建立時間由新到舊排序)
                Order = new List<QueryOrder>()
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            Debug.Log("找到大廳數量: " + queryResponse.Results.Count);
            foreach(Lobby lobby in queryResponse.Results)
            {
                Debug.Log("大廳名稱: " + lobby.Name + " 玩家數量: " + lobby.Players.Count);
            }

            OnListLobbies?.Invoke(this, new OnListLobbiesArgs { LobbyList = queryResponse.Results });

        }catch( LobbyServiceException e)
        {
            Debug.Log("查詢大廳失敗: " + e);
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
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id,joinLobbyByIdOptions);

            Debug.Log("成功加入大廳 Lobby Id: " + lobby.Id);

        }catch (LobbyServiceException e)
        {
            Debug.Log("加入大廳失敗: " + e);
        }
    }

    
    private void PrintPlayers(Lobby lobby)
    {
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Data["PlayerName"].Value);
        }
        OnPrintPlayers.Invoke(this,new OnPrintPlayerArgs{lobby = lobby});
    }

    private Player GetPlayer()
    {
        return new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {
                            "PlayerName",
                            new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName)
                        }
                    }
                };                    
    }
    private void UpdatePlayerName(string newPlayerName)
    {
        try{
            playerName = newPlayerName;
            LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id,AuthenticationService.Instance.PlayerId,new UpdatePlayerOptions
            {
            Data = new Dictionary<string, PlayerDataObject>
                        {
                            {
                                "PlayerName",
                                new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName)
                            }
                        }
            });
        }
        catch(LobbyServiceException e)
        {
            Debug.Log("更新Player Name 失敗: "+e);
        }
        
    }

    private void LeaveLobby()
    {
        try
        {
            LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id,AuthenticationService.Instance.PlayerId);
            hostLobby = null;
            joinedLobby = null;
        }
        catch(LobbyServiceException e)
        {
            Debug.Log("離開大廳失敗: "+e);
        }
    }
    private void KickPlayer(Player player)
    {
        try
        {
            LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id,player.Id);
        }
        catch(LobbyServiceException e)
        {
            Debug.Log("踢除玩家失敗: "+e);
        }
    }
    public bool IsHost()
    {
        return joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }
    public void CreatLobbyButtonClick(int maxPlayers,string lobbyName,string playerName)
    {
        if(playerName!=""||playerName!=null)
            this.playerName = playerName;
        CreatLobby(maxPlayers,lobbyName);
    }
    public void LeaveLobbyButtOnClick()
    {
        LeaveLobby();
        ListLobbies();
    }
    public void JoinLobbyButtonOnClick(Lobby lobby,String playerName)
    {
        if(playerName!=""||playerName!=null)
            this.playerName = playerName;
        JoinLobbyById(lobby);
    }
    public void ListLobbyButtonOnClick()
    {
        ListLobbies();
    }
    public async void OnGameStart()
    {
        await CreatRelayAndUpdateLobbyData();
        GameManager.Instance.StartGame(true);
    }
    public string GetPlayerName()
    {
        return playerName;
    }
}
