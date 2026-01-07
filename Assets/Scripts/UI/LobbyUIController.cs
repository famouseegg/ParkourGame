using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 大廳 UI 控制器 - 統一管理所有大廳相關的 UI 顯示
/// 使用狀態模式切換不同的 UI 面板
/// </summary>
public class LobbyUIController : Singleton<LobbyUIController>
{
    [Header("UI 面板")]
    [SerializeField] private LobbyListUI lobbyListUI;
    [SerializeField] private CreatLobbyUI creatLobbyUI;
    [SerializeField] private PlayerListUI playerListUI;

    private State currentState;
    private List<UIPanel> uiList = new List<UIPanel>();

    /// <summary>
    /// UI 狀態枚舉
    /// </summary>
    public enum State
    {
        NULL,
        LobbyList,      // 大廳列表
        CreatLobbyUI,   // 創建大廳
        PlayerList,     // 玩家列表
        HideAll         // 隱藏所有
    }

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        // 檢查 UI 引用
        if (lobbyListUI == null)
            Debug.LogError("[LobbyUIController] LobbyListUI 未設置！請在 Inspector 中設置引用。");
        if (creatLobbyUI == null)
            Debug.LogError("[LobbyUIController] CreatLobbyUI 未設置！請在 Inspector 中設置引用。");
        if (playerListUI == null)
            Debug.LogError("[LobbyUIController] PlayerListUI 未設置！請在 Inspector 中設置引用。");

        // 初始化 UI 列表
        uiList.Clear();
        if (lobbyListUI != null) uiList.Add(lobbyListUI);
        if (creatLobbyUI != null) uiList.Add(creatLobbyUI);
        if (playerListUI != null) uiList.Add(playerListUI);

        // 設置初始狀態
        ChangeUI(State.LobbyList);
    }

    /// <summary>
    /// 獲取當前狀態
    /// </summary>
    public State Getstate()
    {
        return currentState;
    }

    /// <summary>
    /// 切換 UI 狀態
    /// </summary>
    /// <param name="newState">要切換到的狀態</param>
    /// <param name="isHost">是否為 Host（僅在 PlayerList 狀態使用）</param>
    public void ChangeUI(State newState, bool isHost = false)
    {
        // 避免重複切換
        if (currentState == newState)
            return;

        currentState = newState;

        switch (newState)
        {
            case State.NULL:
                Debug.LogWarning("[LobbyUIController] 切換到 NULL 狀態");
                break;

            case State.LobbyList:
                UpdateUI(lobbyListUI);
                break;

            case State.CreatLobbyUI:
                UpdateUI(creatLobbyUI);
                break;

            case State.PlayerList:
                UpdateUI(playerListUI);
                if (!isHost)
                {
                    playerListUI.HideStartButton();
                }
                break;

            case State.HideAll:
                HideAllUI();
                Debug.Log("[LobbyUIController] 隱藏所有 UI");
                break;

            default:
                Debug.LogError($"[LobbyUIController] 未處理的狀態: {newState}");
                break;
        }
    }

    /// <summary>
    /// 更新 UI 顯示 - 顯示指定的 UI，隱藏其他
    /// </summary>
    private void UpdateUI(UIPanel showUI)
    {
        if (showUI == null)
        {
            Debug.LogError("[LobbyUIController] 嘗試顯示的 UI 為 null！");
            return;
        }

        foreach (UIPanel panel in uiList)
        {
            if (panel == null) continue;

            if (panel == showUI)
            {
                panel.Show();
            }
            else
            {
                panel.Hide();
            }
        }
    }

    /// <summary>
    /// 隱藏所有 UI
    /// </summary>
    private void HideAllUI()
    {
        foreach (UIPanel panel in uiList)
        {
            panel.Hide();
        }
    }

    /// <summary>
    /// 顯示大廳列表
    /// </summary>
    public void ShowLobbyList()
    {
        ChangeUI(State.LobbyList);
    }

    /// <summary>
    /// 顯示創建大廳介面
    /// </summary>
    public void ShowCreateLobby()
    {
        ChangeUI(State.CreatLobbyUI);
    }

    /// <summary>
    /// 顯示玩家列表
    /// </summary>
    public void ShowPlayerList(bool isHost)
    {
        ChangeUI(State.PlayerList, isHost);
    }

    /// <summary>
    /// 隱藏所有 UI（通常在進入遊戲時使用）
    /// </summary>
    public void HideAll()
    {
        ChangeUI(State.HideAll);
    }
}
