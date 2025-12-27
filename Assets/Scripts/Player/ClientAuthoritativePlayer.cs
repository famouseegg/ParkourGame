using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Collections;

public class ClientAuthoritativePlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private StarterAssetsInputs starterInputs;
    [SerializeField] private PlayerMove playerMove;
    [SerializeField] private TextMeshProUGUI playerNameText;

    // 玩家名字（由 Server 寫，Everyone 讀）
    private NetworkVariable<FixedString32Bytes> playerName =
        new NetworkVariable<FixedString32Bytes>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            EnableLocalPlayer();

            // 把 Lobby 的名字送給 Server
            SetNameServerRpc(LobbyManager.Instance.GetPlayerName());
        }
        else
        {
            DisableAll();
        }

        // 所有人都要監聽名字變化
        playerName.OnValueChanged += OnNameChanged;

        // Late join 時也要顯示
        playerNameText.text = playerName.Value.ToString();
    }

    public override void OnNetworkDespawn()
    {
        playerName.OnValueChanged -= OnNameChanged;
    }

    private void OnNameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        playerNameText.text = newValue.ToString();
    }

    [ServerRpc]
    private void SetNameServerRpc(string name)
    {
        playerName.Value = name;
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