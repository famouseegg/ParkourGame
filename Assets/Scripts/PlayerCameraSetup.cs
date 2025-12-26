using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerCameraSetup : NetworkBehaviour
{
    [SerializeField] private Transform cameraTarget;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        CinemachineCamera cam = FindAnyObjectByType<CinemachineCamera>();

        if (cam != null)
        {
            cam.Follow = cameraTarget;
            cam.LookAt = cameraTarget;
        }
        else
        {
            Debug.LogError("Scene 中找不到 CinemachineCamera");
        }
    }
}
