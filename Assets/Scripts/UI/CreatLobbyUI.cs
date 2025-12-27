using System;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;
public class CreatLobbyUI : LobbyUI
{
    [SerializeField] private TMPro.TMP_InputField inputField;
    [SerializeField] private TMPro.TextMeshProUGUI maxtPlayerText;
    [SerializeField] private Button changeMaxPlayerButton;
    [SerializeField] private Button creatButton;
    [SerializeField] private Button backButton;

    [SerializeField] private TMPro.TMP_InputField playerNameInputField;

    private const int MAX_MaxPlayerNumber = 4;
    private const int MIN_MaxPlayerNumber = 1;
    private int maxPlayers;
    private void Start()
    {
        Hide();
        maxPlayers = MIN_MaxPlayerNumber;
        UpdateMaxPlayerText();
        changeMaxPlayerButton.onClick.AddListener(OnchangeMaxPlayerButtonClick);
        creatButton.onClick.AddListener(OnCreatLobbyButtonClick);
        backButton.onClick.AddListener(OnBackButtonClick);
        
    }
    private void OnCreatLobbyButtonClick()
    {
        LobbyManager.Instance.CreatLobbyButtonClick(maxPlayers,inputField.text,playerNameInputField.text);
        LobbyUIController.Instance.ChangeUI(LobbyUIController.State.PlayerList,true);
    }
    private void OnchangeMaxPlayerButtonClick()
    {
        maxPlayers += 1;
        if(maxPlayers>MAX_MaxPlayerNumber)
            maxPlayers = MIN_MaxPlayerNumber; 
        UpdateMaxPlayerText();
    }
    private void OnBackButtonClick()
    {
        LobbyUIController.Instance.ChangeUI(LobbyUIController.State.LobbyList);
    }
    private void UpdateMaxPlayerText()
    {
        maxtPlayerText.text = $"Max Player : {maxPlayers}";
    }
}
