using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClientPlayerMove : NetworkBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private StarterAssetsInputs starterAssetsInputs;
    [SerializeField] private PlayerMove playerMove;
    private void Awake()
    {
        playerInput.enabled = false;
        starterAssetsInputs.enabled =false;
        playerMove.enabled =false;
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            playerInput.enabled = true;
            starterAssetsInputs.enabled =true;
            playerMove.enabled =true;
        }
    }
}
