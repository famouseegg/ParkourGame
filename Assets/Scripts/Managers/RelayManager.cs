//RelayManager.cs
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

/// <summary>
/// Relay 管理器 - 持久化跨場景，負責 Unity Relay Service 的所有操作
/// </summary>
public class RelayManager : PersistentSingleton<RelayManager>
{
    private const int MAX_PLAYERS = 3; // 自己外加三名玩家

    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// 創建 Relay 分配並返回 Join Code
    /// </summary>
    public async Task<string> CreatRelay()
    {
        try
        {
            // 建立 Relay 分配
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MAX_PLAYERS);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // 配置 Unity Transport 使用 Relay
            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("[RelayManager] Unity Transport 組件不存在！");
                return null;
            }

            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            Debug.Log($"[RelayManager] Relay 分配已建立，Join Code: {joinCode}");
            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"[RelayManager] 創建 Relay 分配失敗: {e}");
            return null;
        }
    }

    /// <summary>
    /// 加入 Relay 分配
    /// </summary>
    public async Task JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log($"[RelayManager] 正在加入 Relay，Join Code: {joinCode}");

            // 加入 Relay 分配
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // 詳細日誌
            Debug.Log($"[RelayManager] JoinAllocation 詳情:");
            Debug.Log($"  - RelayServer: {joinAllocation.RelayServer.IpV4}:{joinAllocation.RelayServer.Port}");
            Debug.Log($"  - AllocationId Length: {joinAllocation.AllocationIdBytes?.Length ?? 0}");
            Debug.Log($"  - Key Length: {joinAllocation.Key?.Length ?? 0}");
            Debug.Log($"  - ConnectionData Length: {joinAllocation.ConnectionData?.Length ?? 0}");
            Debug.Log($"  - HostConnectionData Length: {joinAllocation.HostConnectionData?.Length ?? 0}");

            // 驗證數據完整性
            if (joinAllocation.AllocationIdBytes == null || joinAllocation.Key == null ||
                joinAllocation.ConnectionData == null || joinAllocation.HostConnectionData == null)
            {
                Debug.LogError("[RelayManager] Relay 分配數據不完整！");
                return;
            }

            // 配置 Unity Transport 使用 Relay
            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("[RelayManager] Unity Transport 組件不存在！");
                return;
            }

            transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            Debug.Log("[RelayManager] 成功加入 Relay 分配，Transport 已配置");

            // 重要：等待 Transport 完全初始化
            await Task.Delay(200);
            Debug.Log("[RelayManager] Transport 初始化完成");
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"[RelayManager] 加入 Relay 分配失敗: {e}");
            throw; // 重新拋出異常，讓調用者知道失敗了
        }
    }
}
