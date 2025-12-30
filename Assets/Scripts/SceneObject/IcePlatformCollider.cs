using Unity.Netcode;
using UnityEngine;

public class IcePlatformCollider : NetworkBehaviour
{
    [SerializeField] private float reducedSpeedDecelerationRate = 5f;
    [SerializeField] private LayerMask playerMask;

    private void OnTriggerEnter(Collider other)
    {
        if((1<<other.gameObject.layer & playerMask) != 0)
        {
            
           ChangeSpeedDecelerationRate(other, reducedSpeedDecelerationRate);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if((1<<other.gameObject.layer & playerMask) != 0)
        {
            ChangeSpeedDecelerationRate(other, -reducedSpeedDecelerationRate);
        }
    }
    private void ChangeSpeedDecelerationRate(Collider other, float changeRate)
    {
        var playerMove = other.GetComponent<PlayerMove>();
            if (playerMove == null) return;

            //恢復原本的減速速率
            playerMove.ReduceSpeedDecelerationRateClientRpc(
                changeRate,
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
