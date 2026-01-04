using System;
using System.Xml.Serialization;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerListUI : LobbyUI
{
    public static PlayerListUI Instance;
    [SerializeField] private Button StartGameButton;
    [SerializeField] private Button LeaveButton;
    [SerializeField] private Transform container;
    [SerializeField] private Transform SinglePlayerNameListTemplate;
    [SerializeField] private TextMeshProUGUI LobbyNameText;

    private void Awake()
    {
        if (Instance != null)
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

    private void LobbyManager_OnPrintPlayers(object sender, LobbyManager.OnPrintPlayerArgs e)
    {
        UpdateVisuals(e.lobby);
    }

    private void UpdateVisuals(Lobby lobby)
    {
        // 顯示大廳名稱
        if (LobbyNameText != null)
            LobbyNameText.text = lobby.Name;
        // 先複製一份清單，避免遍歷時物件被刪除導致錯誤
        var toDestroy = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in container)
        {
            if (child == SinglePlayerNameListTemplate) continue;
            toDestroy.Add(child);
        }
        foreach (var child in toDestroy)
        {
            if (child != null)
                Destroy(child.gameObject);
        }

        foreach (Player player in lobby.Players)
        {
            Transform SinglePlayerTransform = Instantiate(SinglePlayerNameListTemplate, container);
            SinglePlayerTransform.gameObject.SetActive(true);
            SinglePlayerNameListTempletUI singlePlayerNameListTempletUI = SinglePlayerTransform.GetComponent<SinglePlayerNameListTempletUI>();
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
