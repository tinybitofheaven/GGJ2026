using Unity.Netcode;
using UnityEngine;

public class NPCData : NetworkBehaviour
{
    [SerializeField] private NPCMovement movement;
    [SerializeField] private GameObject model;
    [SerializeField] private SpriteRenderer maskSpriteRenderer;
    [SerializeField] private NetworkObject networkObject;

    private NetworkVariable<int> maskId = new NetworkVariable<int>();
    
    public void SetMsak(int id)
    {
        if (IsServer)
        {
            maskId.Value = id;
        }
        
        ApplyMask(maskId.Value);
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        Debug.Log($"{networkObject.name} spawned, image: {maskId.Value}");
        maskId.OnValueChanged += OnMaskChanged;
        
        // Apply initial mask on clients
        if (!IsServer)
        {
            ApplyMask(maskId.Value);
        }
    }
    
    private void OnMaskChanged(int oldValue, int newValue)
    {
        ApplyMask(newValue);
    }

    public NetworkObject GetNetworkObject()
    {
        return networkObject;
    }

    private void ApplyMask(int id)
    {
        maskSpriteRenderer.sprite = GameManager.Instance.MaskDatabase.GetSpriteById(id);
    }
}
