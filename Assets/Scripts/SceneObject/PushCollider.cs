using System;
using Unity.Netcode;
using UnityEngine;

public class PushCollider : NetworkBehaviour
{
    [SerializeField] private float pushForce = 24f;
    [SerializeField] private float upwardForce = 6f;
    [SerializeField] private LayerMask playerMask;
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if ((1 << other.gameObject.layer & playerMask) != 0)
        {
            PlayerMove player = other.gameObject.GetComponent<PlayerMove>();
            //避免撞到其他物件

            if (player == null) return;

            // 推開方向（由長條指向玩家）
            Vector3 pushDir = (other.gameObject.transform.position - transform.position).normalized;

            Vector3 force = pushDir * pushForce;

            //純水平推容易卡地板
            force.y = upwardForce;

            player.ApplyKnockbackClientRpc(force);
        }

    }
}
