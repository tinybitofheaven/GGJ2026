using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NPCData : NetworkBehaviour
{
    [SerializeField] private NPCMovement movement;
    [SerializeField] private GameObject model;
    [SerializeField] private SpriteRenderer maskSpriteRenderer;
    [SerializeField] private NetworkObject networkObject;

    private NetworkVariable<FixedString32Bytes> npcName = new NetworkVariable<FixedString32Bytes>("John Smith");
    private NetworkVariable<int> npcNumber = new NetworkVariable<int>(0);
    private NetworkVariable<int> maskId = new NetworkVariable<int>();
    
    public void SetMsak(int id)
    {
        if (IsServer)
        {
            maskId.Value = id;
        }
        
        ApplyMask(maskId.Value);
    }
    
    public void SetNumber(int num)
    {
        if (IsServer)
        {
            npcNumber.Value = num;
        }
    }
    
    public void SetName(string n)
    {
        if (IsServer)
        {
            npcName.Value = n;
        }
    }

    public int GetNumber() { return npcNumber.Value; }
    
    public int GetMaskId() { return maskId.Value; }
    
    public FixedString32Bytes GetName()
    {
        return npcName.Value;
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
        Debug.Log($"{networkObject.name} applying mask, image: {maskId.Value}");
        maskSpriteRenderer.sprite = GameManager.Instance.MaskDatabase.GetSpriteById(id);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.CompareTag("EjectZone"))
        {
            RequestDestructionServerRpc();
        }
    }
    
    [ServerRpc]
    private void RequestDestructionServerRpc()
    {
        GameManager.Instance.EvictNPC(this);
    }
}
