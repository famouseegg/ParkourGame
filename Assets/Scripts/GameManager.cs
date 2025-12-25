using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    public event EventHandler OnGameStarted;

    private void Awake()
    {
        if(Instance != null)
            Debug.Log("More than one GameManager instance found!");
        else
            Instance = this; 
    }
    public void StartGame(bool isHost)
    {
        if(isHost)
            NetworkManager.Singleton.StartHost();
        else
            NetworkManager.Singleton.StartClient();
                
        OnGameStarted.Invoke(this,EventArgs.Empty);
    }
}

