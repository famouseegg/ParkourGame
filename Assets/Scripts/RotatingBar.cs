using Unity.Netcode;
using UnityEngine;

public class RotatingBar : NetworkBehaviour
{
    [SerializeField] private float rotationSpeed = 90f;

    public override void OnNetworkSpawn()
    {
        // 確保只有 Server 在轉
        if (!IsServer)
            enabled = false;
    }

    private void Update()
    {
        if(!IsServer)return;
        
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}

