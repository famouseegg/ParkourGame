using Unity.Netcode;
using UnityEngine;

public class SpawnPlateformCollider : NetworkBehaviour
{
    [SerializeField] private LayerMask playerMask;

    private void OnTriggerEnter(Collider other)
    {
        if((1<<other.gameObject.layer & playerMask) != 0)
        {
            
           var respawn = other.GetComponent<Respawn>();
            if (respawn == null) return;

            respawn.SetSpawnPointClientRpc(
                transform.position,
                new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new[] { respawn.OwnerClientId }
                    }
                }
            );
        }
    }
}
