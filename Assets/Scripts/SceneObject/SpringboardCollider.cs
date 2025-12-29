using System;
using UnityEngine;

public class SpringboardCollider : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private LayerMask playerMask;

    [SerializeField] private float launchStrength = 30f;

    private void OnTriggerEnter(Collider other)
    {
        if((1<<other.gameObject.layer & playerMask) != 0)
        {
            CharacterController controller = other.gameObject.GetComponent<CharacterController>();
            PlayerMove player = other.GetComponent<PlayerMove>();
            player.Launch(launchStrength);
            
        }
    }
}
