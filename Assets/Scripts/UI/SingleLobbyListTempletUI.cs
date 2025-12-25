using UnityEngine;
using Unity.Services.Lobbies.Models;
public class SingleLobbyListTempletUI : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI lobbyNameText;
    [SerializeField] private TMPro.TextMeshProUGUI playersText;
    [SerializeField] private UnityEngine.UI.Button joinLobbyButton;
    public void UpdateVisuals(Lobby lobby)
    {
        lobbyNameText.text = lobby.Name;
        playersText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        joinLobbyButton.onClick.AddListener(() => LobbyManager.Instance.JoinLobby(lobby));
    }
    
}
