using UnityEngine;
using System;

public class DefaulReSpawnPoint : MonoBehaviour
{
    public static DefaulReSpawnPoint Instance { get; private set; }
    public static event Action<Vector3> OnSpawnPointReady;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("DefaultSpawnPoint 已存在 Instance");
        }
        else
            Instance = this;
        OnSpawnPointReady?.Invoke(transform.position);
    }

    public Transform GetTransform()
    {
        return transform;
    }
}
