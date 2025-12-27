using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClientAuthoritativePlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private StarterAssetsInputs starterInputs;
    [SerializeField] private PlayerMove playerMove;

  

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            EnableLocalPlayer();
        }
        else
        {
            DisableAll();
        }
    }

    private void EnableLocalPlayer()
    {
        playerInput.enabled = true;
        starterInputs.enabled = true;
        playerMove.enabled = true;
    }

    private void DisableAll()
    {
        if (playerInput != null) playerInput.enabled = false;
        if (starterInputs != null) starterInputs.enabled = false;
        if (playerMove != null) playerMove.enabled = false;
    }
}
