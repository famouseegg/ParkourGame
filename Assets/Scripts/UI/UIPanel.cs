using UnityEngine;

/// <summary>
/// UI 面板基類 - 提供顯示/隱藏功能
/// 適用於所有需要切換顯示的 UI 面板
/// </summary>
public class UIPanel : MonoBehaviour
{
    /// <summary>
    /// 顯示此 UI 面板
    /// </summary>
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 隱藏此 UI 面板
    /// </summary>
    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 檢查面板是否顯示中
    /// </summary>
    public bool IsVisible()
    {
        return gameObject.activeSelf;
    }
}
