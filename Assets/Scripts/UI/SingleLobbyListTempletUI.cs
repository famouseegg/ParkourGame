using UnityEngine;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 單個大廳列表項目 UI - 顯示單個大廳的資訊和加入按鈕
/// </summary>
public class SingleLobbyListTempletUI : MonoBehaviour
{
    [Header("UI 組件")]
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playersText;
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private TMP_InputField playerNameInputField;

    private Lobby lobby;

    private void OnDestroy()
    {
        // 移除按鈕監聽避免內存洩漏
        if (joinLobbyButton != null)
        {
            joinLobbyButton.onClick.RemoveAllListeners();
        }
    }

    /// <summary>
    /// 更新顯示內容
    /// </summary>
    public void UpdateVisuals(Lobby lobby)
    {
        if (lobby == null) return;

        this.lobby = lobby;

        // 顯示大廳名稱
        if (lobbyNameText != null)
        {
            lobbyNameText.text = lobby.Name;
        }

        // 顯示玩家數量
        if (playersText != null)
        {
            playersText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        }

        // 設置按鈕監聽
        if (joinLobbyButton != null)
        {
            joinLobbyButton.onClick.RemoveAllListeners();
            joinLobbyButton.onClick.AddListener(JoinLobbyButtonOnClick);
        }
    }

    private void JoinLobbyButtonOnClick()
    {
        if (lobby == null)
        {
            Debug.LogWarning("[SingleLobbyListTempletUI] 大廳資訊為空，無法加入");
            return;
        }

        string playerName = playerNameInputField != null ? playerNameInputField.text : "";

        if (LobbyManager.Instance != null)
        {
            // 訂閱大廳加入事件
            LobbyManager.Instance.OnLobbyJoined += OnLobbyJoinedSuccess;

            // 加入大廳（異步）
            LobbyManager.Instance.JoinLobbyButtonOnClick(lobby, playerName);

            Debug.Log($"[SingleLobbyListTempletUI] 加入大廳: {lobby.Name}");
        }
    }

    private void OnLobbyJoinedSuccess(object sender, System.EventArgs e)
    {
        // 取消訂閱
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnLobbyJoined -= OnLobbyJoinedSuccess;
        }

        // 根據 Host/Client 狀態切換 UI，避免 UI 狀態殘留
        if (LobbyUIController.Instance != null && LobbyManager.Instance != null)
        {
            bool isHost = LobbyManager.Instance.IsHost();
            LobbyUIController.Instance.ShowPlayerList(isHost);
        }
    }
}
