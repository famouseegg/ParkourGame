using System;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;
public class CreatLobbyUI : MonoBehaviour
{
    public static CreatLobbyUI Instance;
    [SerializeField] private TMPro.TMP_InputField inputField;
    [SerializeField] private TMPro.TextMeshProUGUI maxtPlayerText;
    [SerializeField] private Button changeMaxPlayerButton;
    [SerializeField] private Button creatButton;
    [SerializeField] private Button backButton;

    private const int MAX_MaxPlayerNumber = 4;
    private const int MIN_MaxPlayerNumber = 1;
    private int maxPlayers;
    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning("多個 CreatLobbyUI 實例存在於場景中，僅保留一個實例。");
            
        }
        else
        {
            Instance = this;
        }
    }
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
        LobbyManager.Instance.CreatLobbyButtonClick(maxPlayers,inputField.text);
        Hide();
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
        LobbyListUI.Instance.Show();
        Hide();
    }
    private void UpdateMaxPlayerText()
    {
        maxtPlayerText.text = $"Max Player : {maxPlayers}";
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
