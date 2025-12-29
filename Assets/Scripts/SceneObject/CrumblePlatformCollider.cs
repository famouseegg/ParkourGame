using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class CrumblePlatformCollider : NetworkBehaviour
{
    [SerializeField] private GameObject platformVisual;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private float crumbleTime = 0.5f;
    [SerializeField] private float respawnTime = 2.0f;

    // NetworkVariable 預設為全體可讀，伺服器可寫
    private NetworkVariable<bool> isActive = new NetworkVariable<bool>(true);
    
    // 只在伺服器端標記，不需要同步
    private bool isTriggered = false;

    public override void OnNetworkSpawn()
    {
        if (platformVisual != null)
            platformVisual.SetActive(isActive.Value);


        isActive.OnValueChanged += OnStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        isActive.OnValueChanged -= OnStateChanged;
    }

    private void OnStateChanged(bool previousValue, bool newValue)
    {
        if (platformVisual != null)
            platformVisual.SetActive(newValue);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 檢查是否碰撞到玩家層級
        if (((1 << other.gameObject.layer) & playerMask) != 0)
        {
            // 如果是 Client 踩到，發送 RPC 給伺服器
            if (IsClient)
            {
                TriggerCrumbleServerRpc();
            }
            // 如果是 Server 自己踩到（例如 Host）
            else if (IsServer)
            {
                StartCrumble();
            }
        }
    }

    // 客戶端呼叫此 RPC 來通知伺服器開始崩塌
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void TriggerCrumbleServerRpc()
    {
        StartCrumble();
    }

    private void StartCrumble()
    {
        if (isTriggered) return;
        isTriggered = true;
        StartCoroutine(CrumbleRoutine());
    }

    private IEnumerator CrumbleRoutine()
    {
        // 等待崩塌時間
        yield return new WaitForSeconds(crumbleTime);
        isActive.Value = false; 

        // 等待重生時間
        yield return new WaitForSeconds(respawnTime);
        
        isActive.Value = true;
        isTriggered = false; 
    }
}
