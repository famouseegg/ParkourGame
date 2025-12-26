using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListUI : LobbyUI
{
    [SerializeField] private Button creatLobbyButton;
    [SerializeField] private Button listLobbysButton;
    [SerializeField] private Transform container;
    [SerializeField] private Transform SingleLobbyListTemplate;

    private void Start()
    {
        SingleLobbyListTemplate.gameObject.SetActive(false);
        LobbyManager.Instance.OnListLobbies += LobbyManager_OnListLobbies;
        creatLobbyButton.onClick.AddListener(OnCreateLobbyButtonClicked);
        listLobbysButton.onClick.AddListener(OnListLobbiesButtonClicked);   
        
    }
    
    private void OnCreateLobbyButtonClicked()
    {
        LobbyUIController.Instance.ChangeUI(LobbyUIController.State.CreatLobbyUI);
    }

    private void OnListLobbiesButtonClicked()
    {
        LobbyManager.Instance.ListLobbyButtonOnClick();
    }
    private void LobbyManager_OnListLobbies(object sender, LobbyManager.OnListLobbiesArgs e)
    {
        UpdateVisuals(e.LobbyList);
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
