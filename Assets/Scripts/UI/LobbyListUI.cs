using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyListUI : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private Transform SingleLobbyTemplate;

    private void Start()
    {
        SingleLobbyTemplate.gameObject.SetActive(false);
        LobbyManager.Instance.OnListLobbies += LobbyManager_OnListLobbies;
    }

    private void LobbyManager_OnListLobbies(object sender, LobbyManager.OnListLobbiesArgs e)
    {
        UpdateVisuals(e.LobbyList);
    }
    private void UpdateVisuals(List<Lobby> LobbyList)
    {
        foreach(Transform child in container)
        {
            if(child == SingleLobbyTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach(Lobby lobby in LobbyList)
        {
            Transform SingleLobbyTransform = Instantiate(SingleLobbyTemplate, container);
            SingleLobbyTransform.gameObject.SetActive(true);
            SingleLobbyUI singleLobbyUI = SingleLobbyTransform.GetComponent<SingleLobbyUI>();
            singleLobbyUI.UpdateVisuals(lobby);
        }
    }
}
