using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public List<NetworkObject> connectedPlayers = new ();
    
    private NetworkObject bouncer; // bouncer
    private NetworkObject security; // security guy
        
    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;
        }
    }

    private void OnConnectionEvent(NetworkManager manager, ConnectionEventData eventData)
    {
        if (NetworkManager.Singleton.IsHost && GetPlayerCount() == 2)
        {
            connectedPlayers.Clear();
            foreach (KeyValuePair<ulong, NetworkObject> kvp in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
            {
                // Check if the object is a player object
                if (kvp.Value.IsPlayerObject)
                {
                    connectedPlayers.Add(kvp.Value);
                }
            }
            
            if (Random.value < 0.5f)
            {
                bouncer = connectedPlayers.First();
                security = connectedPlayers.Last();
            }
            else
            {
                security = connectedPlayers.First();
                bouncer = connectedPlayers.Last();
            }
        }
    }
    
    public int GetPlayerCount()
    {
        // This will be accurate on the Host/Server.
        // Clients only see 1 (their own connection) unless synced otherwise.
        return NetworkManager.Singleton.ConnectedClientsList.Count;
    }
}
