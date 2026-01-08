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
    [SerializeField] private Button EscButton;

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (EscButton != null)
        {
            EscButton.onClick.AddListener(OnStartGameScene01Click);
        }
    }

    private void OnStartGameScene01Click()
    {
        StartGameWithScene("LobbyScene");
    }

    private void StartGameWithScene(string sceneName)
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnGameStartWithScene(sceneName);
            Debug.Log($"[BackLobby] 開始遊戲，載入場景: {sceneName}");
        }
    }
}