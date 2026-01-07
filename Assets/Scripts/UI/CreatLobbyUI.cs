using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 創建大廳 UI - 處理大廳創建的輸入和邏輯
/// </summary>
public class CreatLobbyUI : UIPanel
{
    [Header("輸入欄位")]
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private TMP_InputField playerNameInputField;

    [Header("最大玩家數設置")]
    [SerializeField] private TextMeshProUGUI maxPlayerText;
    [SerializeField] private Button changeMaxPlayerButton;

    [Header("按鈕")]
    [SerializeField] private Button createButton;
    [SerializeField] private Button backButton;

    private const int MAX_PLAYER_NUMBER = 4;
    private const int MIN_PLAYER_NUMBER = 1;
    private int maxPlayers = MIN_PLAYER_NUMBER;

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        // 初始隱藏
        Hide();

        // 設置初始值
        maxPlayers = MIN_PLAYER_NUMBER;
        UpdateMaxPlayerText();

        // 設置按鈕監聽
        if (changeMaxPlayerButton != null)
        {
            changeMaxPlayerButton.onClick.AddListener(OnChangeMaxPlayerButtonClick);
        }

        if (createButton != null)
        {
            createButton.onClick.AddListener(OnCreateLobbyButtonClick);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClick);
        }
    }

    private void OnCreateLobbyButtonClick()
    {
        string lobbyName = lobbyNameInputField != null ? lobbyNameInputField.text : "";
        string playerName = playerNameInputField != null ? playerNameInputField.text : "";

        if (LobbyManager.Instance != null)
        {
            // 訂閱大廳加入事件
            LobbyManager.Instance.OnLobbyJoined += OnLobbyCreated;

            // 創建大廳（異步）
            LobbyManager.Instance.CreatLobbyButtonClick(maxPlayers, lobbyName, playerName);

            Debug.Log($"[CreatLobbyUI] 創建大廳: {lobbyName}, 最大玩家數: {maxPlayers}");
        }
    }

    private void OnLobbyCreated(object sender, System.EventArgs e)
    {
        // 取消訂閱
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnLobbyJoined -= OnLobbyCreated;
        }

        // 切換到玩家列表 UI
        if (LobbyUIController.Instance != null)
        {
            LobbyUIController.Instance.ShowPlayerList(isHost: true);
        }
    }

    private void OnDestroy()
    {
        // 清理事件訂閱
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnLobbyJoined -= OnLobbyCreated;
        }
    }

    private void OnChangeMaxPlayerButtonClick()
    {
        maxPlayers++;
        if (maxPlayers > MAX_PLAYER_NUMBER)
        {
            maxPlayers = MIN_PLAYER_NUMBER;
        }

        UpdateMaxPlayerText();
    }

    private void OnBackButtonClick()
    {
        if (LobbyUIController.Instance != null)
        {
            LobbyUIController.Instance.ShowLobbyList();
        }
    }

    private void UpdateMaxPlayerText()
    {
        if (maxPlayerText != null)
        {
            maxPlayerText.text = $"Max Player : {maxPlayers}";
        }
    }

    /// <summary>
    /// 重置輸入欄位
    /// </summary>
    public void ResetInputFields()
    {
        if (lobbyNameInputField != null)
        {
            lobbyNameInputField.text = "";
        }

        if (playerNameInputField != null)
        {
            playerNameInputField.text = "";
        }

        maxPlayers = MIN_PLAYER_NUMBER;
        UpdateMaxPlayerText();
    }
}
