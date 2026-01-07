using UnityEngine;

/// <summary>
/// 看向攝影機組件 - 使物件始終面向主攝影機
/// 常用於 UI 文字或標籤
/// </summary>
public class LookAtCamera : MonoBehaviour
{
    [Header("設置")]
    [SerializeField] private bool invertDirection = false;

    private Camera mainCamera;

    private void Start()
    {
        // 快取主攝影機引用
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogWarning("[LookAtCamera] 找不到主攝影機");
        }
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
        {
            // 嘗試重新取得主攝影機
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        // 使物件面向攝影機
        if (invertDirection)
        {
            // 反向面向（背對攝影機）
            transform.LookAt(transform.position - mainCamera.transform.position);
        }
        else
        {
            // 正常面向攝影機
            transform.LookAt(mainCamera.transform);
        }
    }
}
