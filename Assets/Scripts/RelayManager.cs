//RelayManager.cs
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning("多個 RelayManager 實例存在於場景中，僅保留一個實例。");
        }
        else
        {
            Instance = this;
        }
    }
    public async Task<string> CreatRelay()
    {
        try
        {    
            // 建立 Relay 分配 (自己外加三名玩家)
            Allocation allocation =await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>().SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
                );
            
            // NetworkManager.Singleton.StartHost();
            Debug.Log($"Relay 分配已建立，Join Code: {joinCode}");
            return joinCode;

        }catch(RelayServiceException e)
        {
            Debug.LogError($"創建 Relay 分配失敗: {e}");
            return null;
        }
    }

    public async Task JoinRelay(string joinCode)
    {
        try
        {
           
            // 加入 Relay 分配
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>().SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                );

            NetworkManager.Singleton.StartClient();
            Debug.Log("成功加入 Relay 分配");
         
        }catch(RelayServiceException e)
        {
            Debug.LogError($"加入 Relay 分配失敗: {e}");
        }
    }

}
