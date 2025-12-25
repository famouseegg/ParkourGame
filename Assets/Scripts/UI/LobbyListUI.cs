using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListUI : MonoBehaviour
{
    public static LobbyListUI Instance;
    [SerializeField] private Button creatLobbyButton;
    [SerializeField] private Button listLobbysButton;
    [SerializeField] private Transform container;
    [SerializeField] private Transform SingleLobbyListTemplate;
    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning("多個 LobbyListUI 實例存在於場景中，僅保留一個實例。");   
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        SingleLobbyListTemplate.gameObject.SetActive(false);
        LobbyManager.Instance.OnListLobbies += LobbyManager_OnListLobbies;
        LobbyManager.Instance.OnLeaveLobby += LobbyManager_OnLeaveLobby;
        creatLobbyButton.onClick.AddListener(OnCreateLobbyButtonClicked);
        listLobbysButton.onClick.AddListener(OnListLobbiesButtonClicked);   
        
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnCreateLobbyButtonClicked()
    {
        CreatLobbyUI.Instance.Show();
        Hide();
    }

    private void OnListLobbiesButtonClicked()
    {
        LobbyManager.Instance.ListLobbies();
    }
    private void LobbyManager_OnListLobbies(object sender, LobbyManager.OnListLobbiesArgs e)
    {
        UpdateVisuals(e.LobbyList);
    }
    private void LobbyManager_OnLeaveLobby(object sender, EventArgs e)
    {
        Show();
    }
    private void UpdateVisuals(List<Lobby> LobbyList)
    {
        foreach(Transform child in container)
        {
            if(child == SingleLobbyListTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach(Lobby lobby in LobbyList)
        {
            Transform SingleLobbyTransform = Instantiate(SingleLobbyListTemplate, container);
            SingleLobbyTransform.gameObject.SetActive(true);
            SingleLobbyListTempletUI singleLobbyListTempletUI = SingleLobbyTransform.GetComponent<SingleLobbyListTempletUI>();
            singleLobbyListTempletUI.UpdateVisuals(lobby);
        }
    }
   
}
