using System;
using System.Xml.Serialization;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListUI : LobbyUI
{
    public static PlayerListUI Instance;
    [SerializeField] private Button StartGameButton;
    [SerializeField] private Button LeaveButton;
    [SerializeField] private Transform container;
    [SerializeField] private Transform SinglePlayerNameListTemplate;
    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning("多個 PlayerListUI 實例存在於場景中，僅保留一個實例。");   
        }
        else
        {
            Instance = this;
        }
    }
    private void Start()
    {
        Hide();
        SinglePlayerNameListTemplate.gameObject.SetActive(false);
        LobbyManager.Instance.OnPrintPlayers += LobbyManager_OnPrintPlayers;
        LeaveButton.onClick.AddListener(OnLeaveButtonClick);
        StartGameButton.onClick.AddListener(OnStarGameButtonClick);
    }
    private void LobbyManager_OnPrintPlayers(object sender,LobbyManager.OnPrintPlayerArgs e)
    {
        UpdateVisuals(e.lobby);
    }
    private void UpdateVisuals(Lobby lobby)
    {
        foreach(Transform child in container)
        {
            if(child == SinglePlayerNameListTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach(Player player in lobby.Players)
        {
            Transform SinglePlayerTransform = Instantiate(SinglePlayerNameListTemplate, container);
            SinglePlayerTransform.gameObject.SetActive(true);
            SinglePlayerNameListTempletUI singlePlayerNameListTempletUI= SinglePlayerTransform.GetComponent<SinglePlayerNameListTempletUI>();
            singlePlayerNameListTempletUI.UpdateVisuals(player);
        }
    }
    private void OnLeaveButtonClick()
    {
        LobbyManager.Instance.LeaveLobbyButtOnClick();
        LobbyUIController.Instance.ChangeUI(LobbyUIController.State.LobbyList);
    }
    private void OnStarGameButtonClick()
    {
        LobbyManager.Instance.OnGameStart();
    }
    
    public void HideStartButton()
    {
        StartGameButton.gameObject.SetActive(false);
    }
}
