using UnityEngine;
using Unity.Services.Lobbies.Models;
using TMPro;

/// <summary>
/// 單個玩家名字列表項目 UI - 顯示單個玩家的名字
/// </summary>
public class SinglePlayerNameListTempletUI : MonoBehaviour
{
    [Header("UI 組件")]
    [SerializeField] private TextMeshProUGUI playerText;

    /// <summary>
    /// 更新顯示內容
    /// </summary>
    public void UpdateVisuals(Player player)
    {
        if (player == null)
        {
            Debug.LogWarning("[SinglePlayerNameListTempletUI] 玩家資訊為空");
            return;
        }

        // 顯示玩家名稱
        if (playerText != null && player.Data != null && player.Data.ContainsKey("PlayerName"))
        {
            playerText.text = player.Data["PlayerName"].Value;
        }
        else
        {
            Debug.LogWarning("[SinglePlayerNameListTempletUI] 無法取得玩家名稱");
            if (playerText != null)
            {
                playerText.text = "Unknown Player";
            }
        }
    }
}
