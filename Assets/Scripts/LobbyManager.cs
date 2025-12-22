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
    private Lobby hostLobby;
    public static LobbyManager Instance { get; private set; }
    private float heartbeatTimer;
    public event EventHandler<OnListLobbiesArgs> OnListLobbies;
    public class OnListLobbiesArgs : EventArgs
    {
        public List<Lobby> LobbyList;
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
        
    }

    private void Update()
    {
        HandleHeartbeat();
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

    //
    public async void CreatLobby()
    {
        try
        {
            string lobbyname = "TestLobby";
            int maxPlayers = 4;
            
            // 設定私有大廳
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,                             
            };

            Lobby lobby =await LobbyService.Instance.CreateLobbyAsync(lobbyname, maxPlayers, createLobbyOptions);
            hostLobby = lobby;
            Debug.Log("大廳已建立: " + lobby.Name + "大廳人數上限: "+ lobby.MaxPlayers + "Lobby ID: "+ lobby.Id + "Lobby Code: " + lobby.LobbyCode);
            string relayJoinCode = await RelayManager.Instance.CreatRelay();

            Debug.Log($"GetRelay Join Code: {relayJoinCode}");
            // 更新 Lobby Data（關鍵）
            await LobbyService.Instance.UpdateLobbyAsync(
                hostLobby.Id,
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
            NetworkManager.Singleton.StartHost();

        }catch (LobbyServiceException e)
        {
            Debug.Log("建立大廳失敗: " + e);
        }
        
    }

    public async void ListLobbies()
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

    public async void JoinLobbyById(Lobby lobby)
    {
        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
            // await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            Debug.Log("成功加入大廳 Lobby Id: " + lobby.Id);

            string relayJoinCode =
            lobby.Data["RelayJoinCode"].Value;

            Debug.Log("取得 Relay JoinCode: " + relayJoinCode);

            await RelayManager.Instance.JoinRelay(relayJoinCode);

            NetworkManager.Singleton.StartClient();

        }catch (LobbyServiceException e)
        {
            Debug.Log("加入大廳失敗: " + e);
        }
    }

}
