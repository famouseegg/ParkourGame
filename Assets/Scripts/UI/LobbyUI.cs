using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button creatLobbyButton;
    [SerializeField] private Button listLobbysButton;
   private void Start()
    {
        creatLobbyButton.onClick.AddListener(OnCreateLobbyButtonClicked);
        listLobbysButton.onClick.AddListener(OnListLobbiesButtonClicked);   
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
    }
    private void OnCreateLobbyButtonClicked()
    {
        LobbyManager.Instance.CreatLobby();
    }

    private void OnListLobbiesButtonClicked()
    {
        LobbyManager.Instance.ListLobbies();
    }

    private void GameManager_OnGameStarted(object sender, System.EventArgs e)
    {
        gameObject.SetActive(false);
    }
}
