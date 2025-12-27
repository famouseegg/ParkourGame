using UnityEngine;

public class SpawnPlateformCollider : MonoBehaviour
{
    [SerializeField] private LayerMask playerMask;

    private void OnTriggerEnter(Collider other)
    {
        if((1<<other.gameObject.layer & playerMask) != 0)
        {
            other.gameObject.GetComponent<Respawn>().SetSpawnPoint(transform);
        }
    }
}
