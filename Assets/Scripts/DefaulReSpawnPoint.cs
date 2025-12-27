using UnityEngine;

public class DefaulReSpawnPoint : MonoBehaviour
{
    public static DefaulReSpawnPoint Instance {get;private set;}
    private void Awake()
    {
        if(Instance != null)
        {
            Debug.Log("DefaultSpawnPoint 已存在 Instance");
        }
        else
            Instance = this;
    }
    public Transform GetTransform()
    {
        return transform;
    }
}
