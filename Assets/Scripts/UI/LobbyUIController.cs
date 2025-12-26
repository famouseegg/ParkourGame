using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class LobbyUIController : MonoBehaviour
{
    public static LobbyUIController Instance;
    [SerializeField] private LobbyListUI lobbyListUI;
    [SerializeField] private CreatLobbyUI creatLobbyUI;
    [SerializeField] private PlayerListUI playerListUI;
    public enum State{
        NULL,
        LobbyList,
        CreatLobbyUI,
        PlayerList,
        HideAll
    }
    private State state;
    List<LobbyUI> uiList = new List<LobbyUI>();
    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning("多個 LobbyUIController 實例存在於場景中，僅保留一個實例。");   
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        state = State.LobbyList;
        uiList.Add(lobbyListUI);
        uiList.Add(creatLobbyUI);
        uiList.Add(playerListUI);
    }
    public State Getstate()
    {
        return state;
    }
    public void ChangeUI(State state,bool isHost=false)
    {
        if(this.state == state)
            return;
        else
            this.state = state;
        switch (state)
        {   
            default:
                Debug.Log("LobbyUIController State is null ");
                break;
            case State.LobbyList:
                UpdateUI(lobbyListUI);
                break;
            case State.CreatLobbyUI:
                UpdateUI(creatLobbyUI);
                break;
            case State.PlayerList:
                UpdateUI(playerListUI);
                if (!isHost)
                {
                    playerListUI.HideStartButton();
                }
                break;
            case State.HideAll:
                UpdateUI();
                break;
        }
    }
    private void UpdateUI(LobbyUI showUI)
    {
        foreach(LobbyUI lobbyUI in uiList)
        {
            if(lobbyUI == showUI)
            {
                lobbyUI.Show();
                continue;
            }
            lobbyUI.Hide();
        }
    }
    private void UpdateUI()
    {
        foreach(LobbyUI lobbyUI in uiList)
        {           
            lobbyUI.Hide();
        }
    }
}
