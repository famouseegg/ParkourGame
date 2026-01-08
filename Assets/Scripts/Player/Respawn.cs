using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

/// <summary>
/// 玩家重生系統 - 處理掉落和重生邏輯
/// 已整合場景生命週期管理
/// </summary>
public class Respawn : NetworkBehaviour
{
    [SerializeField] private NetworkTransform networkTransform;
    [SerializeField] private float fallThreshold = -15.0f;

    private CharacterController controller;
    private Vector3 spawnPoint;
    private bool isSpawnPointReady = false;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        controller = GetComponent<CharacterController>();

        // 訂閱重生點事件
        DefaulReSpawnPoint.OnSpawnPointReady += OnSpawnPointReady;

        // 訂閱場景生命週期事件
        if (SceneLifecycleManager.Instance != null)
        {
            SceneLifecycleManager.Instance.OnScenePostLoad += OnSceneLoadComplete;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        // 取消訂閱
        DefaulReSpawnPoint.OnSpawnPointReady -= OnSpawnPointReady;

        if (SceneLifecycleManager.Instance != null)
        {
            SceneLifecycleManager.Instance.OnScenePostLoad -= OnSceneLoadComplete;
        }
    }

    private void OnSceneLoadComplete(string sceneName)
    {
        if (!IsOwner) return;

        // 場景切換後重置狀態
        isSpawnPointReady = false;
        Debug.Log($"[Respawn] 場景 {sceneName} 載入完成，等待重生點就緒");
    }

    private void OnSpawnPointReady(Vector3 point)
    {
        if (!IsOwner) return;

        spawnPoint = point;
        isSpawnPointReady = true;

        if (controller == null)
            controller = GetComponent<CharacterController>();

        // 傳送到重生點
        TeleportToSpawnPoint();

        Debug.Log($"[Respawn] 重生點已設置: {point}");
    }

    private void Start()
    {
        // 延遲到 Start 再檢查，確保場景物件都已初始化
        if (IsOwner && !isSpawnPointReady)
        {
            TryFindSpawnPoint();
        }
    }

    private void Update()
    {
        if (!IsOwner || !isSpawnPointReady) return;

        // 檢查是否掉落
        if (transform.position.y <= fallThreshold)
        {
            DoRespawn();
        }
    }

    private void TryFindSpawnPoint()
    {
        // 嘗試查找預設重生點
        if (DefaulReSpawnPoint.Instance != null)
        {
            Vector3 pos = DefaulReSpawnPoint.Instance.GetTransform().position;
            Debug.Log($"[Respawn] 在 Start 中找到重生點，位置: {pos}");
            OnSpawnPointReady(pos);
        }
        else
        {
            // 如果還是找不到，使用 Coroutine 稍後再試
            StartCoroutine(WaitForSpawnPoint());
        }
    }

    private System.Collections.IEnumerator WaitForSpawnPoint()
    {
        Debug.Log("[Respawn] 等待重生點初始化...");

        float timeout = 5f;
        float elapsed = 0f;

        while (DefaulReSpawnPoint.Instance == null && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        if (DefaulReSpawnPoint.Instance != null)
        {
            Vector3 pos = DefaulReSpawnPoint.Instance.GetTransform().position;
            Debug.Log($"[Respawn] 在 Coroutine 中找到重生點，位置: {pos}");
            OnSpawnPointReady(pos);
        }
        else
        {
            Debug.LogError("[Respawn] 超時！無法找到重生點");
        }
    }

    private void DoRespawn()
    {
        Debug.Log("[Respawn] 玩家掉落，執行重生");
        TeleportToSpawnPoint();
    }

    private void TeleportToSpawnPoint()
    {
        if (controller == null || networkTransform == null) return;

        // Character Controller 會影響傳送，須暫時關閉
        controller.enabled = false;

        networkTransform.Teleport(spawnPoint, transform.rotation, transform.localScale);

        controller.enabled = true;
    }

    /// <summary>
    /// Server RPC: 設置重生點
    /// </summary>
    [ServerRpc]
    public void SetSpawnPointServerRpc(Vector3 position)
    {
        SetSpawnPointClientRpc(position, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { OwnerClientId }
            }
        });
    }

    /// <summary>
    /// Client RPC: 接收重生點設置
    /// </summary>
    [ClientRpc]
    public void SetSpawnPointClientRpc(Vector3 position, ClientRpcParams rpcParams = default)
    {
        if (!IsOwner) return;

        spawnPoint = position;
        isSpawnPointReady = true;
        Debug.Log($"[Respawn] 收到重生點設置: {position}");
    }

    /// <summary>
    /// 手動觸發重生（調試用）
    /// </summary>
    public void ManualRespawn()
    {
        if (!IsOwner || !isSpawnPointReady) return;
        DoRespawn();
    }

    /// <summary>
    /// 檢查重生點是否已就緒
    /// </summary>
    public bool IsSpawnPointReady()
    {
        return isSpawnPointReady;
    }

    /// <summary>
    /// 獲取當前重生點位置
    /// </summary>
    public Vector3 GetSpawnPoint()
    {
        return spawnPoint;
    }
}
