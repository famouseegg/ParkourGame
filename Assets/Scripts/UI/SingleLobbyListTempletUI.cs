using UnityEngine;
using Unity.Services.Lobbies.Models;
public class SingleLobbyListTempletUI : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI lobbyNameText;
    [SerializeField] private TMPro.TextMeshProUGUI playersText;
    [SerializeField] private UnityEngine.UI.Button joinLobbyButton;
    private Lobby lobby;
    public void UpdateVisuals(Lobby lobby)
    {
        this.lobby = lobby;
        lobbyNameText.text = lobby.Name;
        playersText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        joinLobbyButton.onClick.AddListener(JoinLobbyButtonOnClick);
    }
    private void JoinLobbyButtonOnClick()
    {
        LobbyManager.Instance.JoinLobbyButtonOnClick(lobby);
        LobbyUIController.Instance.ChangeUI(LobbyUIController.State.PlayerList,false);
    }
}
