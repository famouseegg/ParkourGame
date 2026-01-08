using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// UI 面板基類 - 提供顯示/隱藏功能
/// 適用於所有需要切換顯示的 UI 面板
/// </summary>
public class UIPanel : MonoBehaviour
{
    private void Update()
    {
        // 監聽 ESC 鍵按下 (使用新的 Input System)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            UnlockCursor();
        }
    }

    /// <summary>
    /// 解鎖滑鼠游標
    /// </summary>
    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

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
