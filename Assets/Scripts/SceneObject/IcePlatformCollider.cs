using Unity.Netcode;
using UnityEngine;

public class IcePlatformCollider : NetworkBehaviour
{
    // 越大的值代表玩家在冰面上的減速時間越長
    [SerializeField] private float extraDecelTime = 0.8f;
    [SerializeField] private LayerMask playerMask;

    private void OnTriggerEnter(Collider other)
    {
        // 檢查是否為玩家
        if ((1 << other.gameObject.layer & playerMask) != 0)
        {
            // 增加玩家的減速時間(變滑)
            ChangeSpeedDecelerationRate(other, extraDecelTime);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((1 << other.gameObject.layer & playerMask) != 0)
        {
            // 恢復玩家的減速時間
            ChangeSpeedDecelerationRate(other, -extraDecelTime);
        }
    }

    private void ChangeSpeedDecelerationRate(Collider other, float adjustment)
    {
        if (!IsServer) return;

        var playerMove = other.GetComponent<PlayerMove>();
        if (playerMove == null) return;

        //恢復原本的減速速率
        playerMove.AdjustDecelTimeClientRpc(
            adjustment,
            new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { playerMove.OwnerClientId }
                }
            }
        );
    }
}
