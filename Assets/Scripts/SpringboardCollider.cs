using System;
using UnityEngine;

public class SpringboardCollider : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private LayerMask playerMask;

    [SerializeField] private float strength=50;

    private void OnTriggerEnter(Collider other)
    {
        if((1<<other.gameObject.layer & playerMask) != 0)
        {
            CharacterController controller = other.gameObject.GetComponent<CharacterController>();
            controller.Move(new Vector3(0.0f,strength,0.0f));
        }
    }
}
