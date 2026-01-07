using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 玩家列表 UI - 顯示當前大廳中的玩家列表
/// </summary>
public class PlayerListUI : UIPanel
{
    private static PlayerListUI _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting = false;

    public static PlayerListUI Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning("[PlayerListUI] Instance 已被銷毀（應用程式正在關閉）");
                return null;
            }

            // Double-Check Locking
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindFirstObjectByType<PlayerListUI>();
                        if (_instance == null)
                        {
                            Debug.LogError("[PlayerListUI] 場景中未找到實例！");
                        }
                    }
                }
            }

            return _instance;
        }
    }

    [Header("UI 組件")]
    [SerializeField] private Button gameScene01Button;
    [SerializeField] private Button gameScene02Button;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Transform container;
    [SerializeField] private Transform singlePlayerNameListTemplate;
    [SerializeField] private TextMeshProUGUI lobbyNameText;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Debug.LogWarning("[PlayerListUI] 檢測到多個實例，銷毀重複實例");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeUI();
        SubscribeEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();

        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    private void InitializeUI()
    {
        // 隱藏模板
        if (singlePlayerNameListTemplate != null)
        {
            singlePlayerNameListTemplate.gameObject.SetActive(false);
        }

        // 設置按鈕監聽
        if (leaveButton != null)
        {
            leaveButton.onClick.AddListener(OnLeaveButtonClick);
        }

        if (gameScene01Button != null)
        {
            gameScene01Button.onClick.AddListener(OnStartGameScene01Click);
        }

        if (gameScene02Button != null)
        {
            gameScene02Button.onClick.AddListener(OnStartGameScene02Click);
        }
    }

    private void SubscribeEvents()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnPrintPlayers += LobbyManager_OnPrintPlayers;
        }
    }

    private void UnsubscribeEvents()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnPrintPlayers -= LobbyManager_OnPrintPlayers;
        }
    }

    private void LobbyManager_OnPrintPlayers(object sender, LobbyManager.OnPrintPlayerArgs e)
    {
        UpdateVisuals(e.lobby);
    }

    private void UpdateVisuals(Lobby lobby)
    {
        if (lobby == null) return;

        // 顯示大廳名稱
        if (lobbyNameText != null)
        {
            lobbyNameText.text = lobby.Name;
        }

        // 清除現有的玩家項目
        ClearPlayerItems();

        // 創建新的玩家項目
        if (lobby.Players != null)
        {
            foreach (Player player in lobby.Players)
            {
                CreatePlayerItem(player);
            }
        }
    }

    private void ClearPlayerItems()
    {
        if (container == null) return;

        // 先收集要銷毀的物件，避免遍歷時修改集合
        var toDestroy = new List<Transform>();
        foreach (Transform child in container)
        {
            if (child == singlePlayerNameListTemplate) continue;
            toDestroy.Add(child);
        }

        // 銷毀物件
        foreach (var child in toDestroy)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void CreatePlayerItem(Player player)
    {
        if (singlePlayerNameListTemplate == null || container == null) return;

        Transform singlePlayerTransform = Instantiate(singlePlayerNameListTemplate, container);
        singlePlayerTransform.gameObject.SetActive(true);

        SinglePlayerNameListTempletUI singlePlayerUI = singlePlayerTransform.GetComponent<SinglePlayerNameListTempletUI>();
        if (singlePlayerUI != null)
        {
            singlePlayerUI.UpdateVisuals(player);
        }
    }

    private void OnLeaveButtonClick()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.LeaveLobbyButtOnClick();
        }

        if (LobbyUIController.Instance != null)
        {
            LobbyUIController.Instance.ShowLobbyList();
        }
    }

    private void OnStartGameScene01Click()
    {
        StartGameWithScene("01-GameScene");
    }

    private void OnStartGameScene02Click()
    {
        StartGameWithScene("02-GameScene");
    }

    private void StartGameWithScene(string sceneName)
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnGameStartWithScene(sceneName);
            Debug.Log($"[PlayerListUI] 開始遊戲，載入場景: {sceneName}");
        }
    }

    /// <summary>
    /// 隱藏開始遊戲按鈕（Client 使用）
    /// </summary>
    public void HideStartButton()
    {
        if (gameScene01Button != null)
        {
            gameScene01Button.gameObject.SetActive(false);
        }

        if (gameScene02Button != null)
        {
            gameScene02Button.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 顯示開始遊戲按鈕（Host 使用）
    /// </summary>
    public void ShowStartButton()
    {
        if (gameScene01Button != null)
        {
            gameScene01Button.gameObject.SetActive(true);
        }

        if (gameScene02Button != null)
        {
            gameScene02Button.gameObject.SetActive(true);
        }

        Debug.Log("[PlayerListUI] 顯示開始遊戲按鈕（Host 模式）");
    }
}
