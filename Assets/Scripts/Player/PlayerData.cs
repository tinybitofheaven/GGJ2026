using System;
using Unity.Netcode;
using UnityEngine;

public enum Role
{
    Bouncer,
    Security,
    NotSet
}

public class PlayerData : NetworkBehaviour
{
    [SerializeField] private GameObject _playerGameObject;
    [SerializeField] private NetworkObject _playerNetworkObject;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private GameObject _playerModel;
    [SerializeField] private GameObject _playerIndicator;
    
    public GameObject PlayerGameObject
    {
        get => _playerGameObject;
        set => _playerGameObject = value;
    }

    public NetworkObject PlayerNetworkObject     
    {
        get => _playerNetworkObject;
        set => _playerNetworkObject = value;
    }
    public PlayerMovement PlayerMovement 
    {
        get => _playerMovement;
        set => _playerMovement = value;
    }
    
    private NetworkVariable<Role> playerRole = new NetworkVariable<Role>(
        Role.NotSet, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );
    public Role PlayerRole => playerRole.Value;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        PlayerGameObject = gameObject;
        PlayerNetworkObject = PlayerGameObject.GetComponent<NetworkObject>();
        PlayerMovement = PlayerGameObject.GetComponent<PlayerMovement>();
        
        // Subscribe to role changes
        playerRole.OnValueChanged += OnRoleChanged;
        
        // Apply current role if already set
        if (playerRole.Value != Role.NotSet)
        {
            ApplyRole(playerRole.Value);
        }
    }
    
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        playerRole.OnValueChanged -= OnRoleChanged;
    }
    
    private void OnRoleChanged(Role previousValue, Role newValue)
    {
        Debug.Log($"Role changed from {previousValue} to {newValue} on {gameObject.name}");
        ApplyRole(newValue);
    }
    public void SetRole(Role role)
    {
        if (!IsServer)
        {
            Debug.LogWarning("Only server can set roles!");
            return;
        }
        
        playerRole.Value = role;
    }

    private void ApplyRole(Role role)
    {
        gameObject.name = "Player " + role;
        
        if (role == Role.Security)
        {
            PlayerMovement.SetDisableMovement(true);
            // _playerModel.SetActive(false);
        }

        // if (IsOwner)
        // {
        //     _playerIndicator.SetActive(true);
        // }
    }

    public Role GetRole()
    {
        return playerRole.Value;
    }

    public bool IsBouncer()
    {
        return playerRole.Value == Role.Bouncer;
    }

    public bool IsSecurity()
    {
        return playerRole.Value == Role.Security;
    }

    public GameObject GetPlayerGameObject()
    {
        return PlayerGameObject;
    }

    public NetworkObject GetPlayerNetworkObject()
    {
        return PlayerNetworkObject;
    }
}
