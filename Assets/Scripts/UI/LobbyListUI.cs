using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 大廳列表 UI - 顯示可用的大廳列表
/// </summary>
public class LobbyListUI : UIPanel
{
    [Header("UI 組件")]
    [SerializeField] private Button creatLobbyButton;
    [SerializeField] private Button listLobbysButton;
    [SerializeField] private Transform container;
    [SerializeField] private Transform singleLobbyListTemplate;

    private void Start()
    {
        InitializeUI();
        SubscribeEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void InitializeUI()
    {
        // 隱藏模板
        if (singleLobbyListTemplate != null)
        {
            singleLobbyListTemplate.gameObject.SetActive(false);
        }

        // 設置按鈕監聽
        if (creatLobbyButton != null)
        {
            creatLobbyButton.onClick.AddListener(OnCreateLobbyButtonClicked);
        }

        if (listLobbysButton != null)
        {
            listLobbysButton.onClick.AddListener(OnListLobbiesButtonClicked);
        }
    }

    private void SubscribeEvents()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnListLobbies += LobbyManager_OnListLobbies;
        }
    }

    private void UnsubscribeEvents()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnListLobbies -= LobbyManager_OnListLobbies;
        }
    }

    private void OnCreateLobbyButtonClicked()
    {
        if (LobbyUIController.Instance != null)
        {
            LobbyUIController.Instance.ShowCreateLobby();
        }
    }

    private void OnListLobbiesButtonClicked()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.ListLobbyButtonOnClick();
        }
    }

    private void LobbyManager_OnListLobbies(object sender, LobbyManager.OnListLobbiesArgs e)
    {
        UpdateVisuals(e.LobbyList);
    }

    private void UpdateVisuals(List<Lobby> lobbyList)
    {
        // 清除現有的大廳項目（保留模板）
        ClearLobbyItems();

        // 創建新的大廳項目
        if (lobbyList != null)
        {
            foreach (Lobby lobby in lobbyList)
            {
                CreateLobbyItem(lobby);
            }
        }
    }

    private void ClearLobbyItems()
    {
        if (container == null) return;

        foreach (Transform child in container)
        {
            if (child == singleLobbyListTemplate) continue;
            Destroy(child.gameObject);
        }
    }

    private void CreateLobbyItem(Lobby lobby)
    {
        if (singleLobbyListTemplate == null || container == null) return;

        Transform singleLobbyTransform = Instantiate(singleLobbyListTemplate, container);
        singleLobbyTransform.gameObject.SetActive(true);

        SingleLobbyListTempletUI singleLobbyUI = singleLobbyTransform.GetComponent<SingleLobbyListTempletUI>();
        if (singleLobbyUI != null)
        {
            singleLobbyUI.UpdateVisuals(lobby);
        }
    }

    /// <summary>
    /// 手動刷新大廳列表
    /// </summary>
    public void RefreshLobbyList()
    {
        OnListLobbiesButtonClicked();
    }
}
