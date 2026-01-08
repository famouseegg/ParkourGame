using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 玩家列表 UI - 顯示當前大廳中的玩家列表
/// </summary>
public class BackLobby : UIPanel
{
    private static BackLobby _instance;

    [Header("UI 組件")]
    [SerializeField] private Button returnLobbyButton;

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (returnLobbyButton != null)
        {
            returnLobbyButton.onClick.AddListener(OnStartGameScene01Click);
        }
    }

    private void OnStartGameScene01Click()
    {
        ReturnToLobby();
    }

    private void ReturnToLobby()
    {
        if (SceneLifecycleManager.Instance != null)
        {
            SceneLifecycleManager.Instance.RequestReturnToLobby();
            Debug.Log("[BackLobby] 請求返回大廳");
        }
    }
}