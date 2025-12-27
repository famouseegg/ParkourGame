using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Respawn : NetworkBehaviour
{
    [SerializeField] private NetworkTransform networkTransform;

    [SerializeField] private float fallThreshold = -15.0f;
    private CharacterController controller;

    private Transform spawnPoint;
    public override void OnNetworkSpawn()
    {
        if(!IsOwner)return;

        spawnPoint = DefaulReSpawnPoint.Instance.GetTransform();
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

        networkTransform.Teleport(spawnPoint.position, spawnPoint.rotation, transform.localScale);

        controller.enabled = true;
    }
    public void SetSpawnPoint(Transform transform)
    {
        spawnPoint=transform;
    }
}
