using Unity.Netcode;
using UnityEngine;

public class RotatingBarPush : NetworkBehaviour
{
    [SerializeField] private float pushForce = 24f;
    [SerializeField] private float upwardForce = 6f;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!IsServer) return;

        PlayerMove player = hit.collider.GetComponent<PlayerMove>();
        //避免撞到其他物件

        if (player == null) return;

        // 推開方向（由長條指向玩家）
        Vector3 pushDir = (hit.collider.transform.position - transform.position).normalized;

        Vector3 force = pushDir * pushForce;

        //純水平推容易卡地板
        force.y = upwardForce;

        player.Knockback(force);
    }
}
