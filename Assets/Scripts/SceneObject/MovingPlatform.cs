using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class MovingPlatform : NetworkBehaviour
{
    [Header("Start position")]
    [SerializeField] private GameObject PointA;
    [SerializeField] private GameObject PointB;
    [SerializeField] private GameObject platformParent;
    [SerializeField] private float speed=10f;
    [SerializeField] private float delay=1f;

    private Vector3 targetPosit;
    public override void OnNetworkSpawn()
    {
        if(!IsServer)
            return;
        platformParent.transform.position = PointA.transform.position;
        targetPosit = PointB.transform.position;
        //模擬非同步
        StartCoroutine(MovePlateform());
        
    }
    IEnumerator MovePlateform()
    {
        while (true)
        {
            while ((targetPosit - platformParent.transform.position).sqrMagnitude > 0.01f)
            {
                platformParent.transform.position = Vector3.MoveTowards(platformParent.transform.position
                    ,targetPosit,speed*Time.deltaTime);
                    yield return null;
            }
            if(targetPosit == PointA.transform.position)
                targetPosit = PointB.transform.position;
            else
                targetPosit = PointA.transform.position;
            yield return new WaitForSeconds(delay);
        }
    }

}
