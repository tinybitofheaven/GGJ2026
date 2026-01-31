using System;
using StarterAssets;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private ThirdPersonController _playerMovement;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private GameObject _playerModel;
    
    
    private StarterAssetsInputs _starterAssetsInputs;
    private PlayerInput _playerInput;
    
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
    public ThirdPersonController PlayerMovement 
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
        PlayerMovement = PlayerGameObject.GetComponentInChildren<ThirdPersonController>();
        _starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        _playerInput = GetComponent<PlayerInput>();
        
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
            // gameObject.SetActive(false);
            SetDisableMovementClientRPC(true);
        }
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
    
    private bool disableMovement = false;
        
    [ClientRpc]
    public void SetDisableMovementClientRPC(bool value)
    {
        if (!IsOwner) return;
    
        disableMovement = value;
        if (disableMovement)
        {
            Debug.Log("Disabled movement for: " + gameObject.name);
            characterController.enabled = false;
            _playerMovement.enabled = false;
            _playerModel.SetActive(false);
            _starterAssetsInputs.enabled = false;
            _playerMovement.enabled = false;
            _playerInput.enabled = false;
        }
        else
        {
            characterController.enabled = true;
            _playerMovement.enabled = true;
            _playerModel.SetActive(true);
            _starterAssetsInputs.enabled = true;
            _playerMovement.enabled = true;
            _playerInput.enabled = true;
        }
    }

    public PlayerInput GetPlayerInput()
    {
        return _playerInput;
    }
    
}
