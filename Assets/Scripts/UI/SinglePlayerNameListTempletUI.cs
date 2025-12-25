using UnityEngine;
using Unity.Services.Lobbies.Models;
public class SinglePlayerNameListTempletUI : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI PlayerText;
    public void UpdateVisuals(Player player)
    {
        PlayerText.text = player.Data["PlayerName"].Value;
       
    }
}
