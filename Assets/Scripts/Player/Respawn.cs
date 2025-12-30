using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Respawn : NetworkBehaviour
{
    [SerializeField] private NetworkTransform networkTransform;

    [SerializeField] private float fallThreshold = -15.0f;
    private CharacterController controller;

    private Vector3 spawnPoint;
    public override void OnNetworkSpawn()
    {
        if(!IsOwner)return;

        spawnPoint = DefaulReSpawnPoint.Instance.GetTransform().position;
        controller = this.GetComponent<CharacterController>();
    }
    private void Update()
    {
        if(!IsOwner) return;
        if(transform.position.y <= fallThreshold)
        {
            DoSpawn();
        }
    }
    private void DoSpawn()
    {
        //Charactor controller 會影響傳送須暫時關閉
        controller.enabled = false;

        networkTransform.Teleport(spawnPoint, transform.rotation, transform.localScale);

        controller.enabled = true;
    }

    [ClientRpc]
    public void SetSpawnPointClientRpc(Vector3 position, ClientRpcParams rpcParams = default)
    {
        if (!IsOwner) return;
        spawnPoint = position;
    }
}
