using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class PlayerGrab : NetworkBehaviour
{
    [Header("Settings")]
    public Transform handPoint;
    public float grabDistance = 1.0f;
    public float grabRadius = 0.5f;
    public LayerMask grabLayer;

    [Header("Sync State")]
    // Synchronizes which object is being held across the network
    private NetworkVariable<NetworkObjectReference> grabbedObjectRef = new NetworkVariable<NetworkObjectReference>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );
    
    public NetworkObjectReference GrabbedObject => grabbedObjectRef.Value;

    private GameObject visualModel;
    private CharacterController characterController;
    
    public Action OnGrab;
    public Action OnRelease;

    private void Start()
    {
        characterController = this.GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"player grab enabled");
        grabbedObjectRef.OnValueChanged += OnGrabbedObjectChanged;
        
        // Handle late-joiners
        if (grabbedObjectRef.Value.TryGet(out NetworkObject netObj))
        {
            UpdateVisuals(netObj.GetComponent<Grabbable>(), true);
        }
    }

    public override void OnNetworkDespawn()
    {
        grabbedObjectRef.OnValueChanged -= OnGrabbedObjectChanged;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (grabbedObjectRef.Value.TryGet(out _))
            {
                RequestDropServerRpc();
            }
            else
            {
                TryGrab();
            }
        }
    }

    private void TryGrab()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, grabRadius, transform.forward, out hit, grabDistance, grabLayer))
        {
            if (hit.collider.TryGetComponent<Grabbable>(out var grabbable))
            {
                RequestGrabServerRpc(grabbable.NetworkObject);
            }
        }
    }

    [ServerRpc]
    private void RequestGrabServerRpc(NetworkObjectReference netObjRef)
    {
        grabbedObjectRef.Value = netObjRef;
    }

    [ServerRpc]
    private void RequestDropServerRpc()
    {
        if (grabbedObjectRef.Value.TryGet(out NetworkObject netObj))
        {
            Vector3 dropPosition = transform.position + (transform.forward * grabDistance);

            // 1. Try to get the NetworkTransform
            if (netObj.TryGetComponent<Unity.Netcode.Components.NetworkTransform>(out var networkTransform))
            {
                // This is the "Magic" function that stops interpolation/sliding
                networkTransform.Teleport(dropPosition, Quaternion.identity, netObj.transform.localScale);
            }
            else
            {
                // Fallback for objects without NetworkTransform
                netObj.transform.position = dropPosition;
            }
        
            grabbedObjectRef.Value = default;
        }
    }

    private void OnGrabbedObjectChanged(NetworkObjectReference previousValue, NetworkObjectReference newValue)
    {
        // CASE: OBJECT WAS JUST GRABBED
        if (newValue.TryGet(out NetworkObject netObj))
        {
            UpdateVisuals(netObj.GetComponent<Grabbable>(), true);
            OnGrab?.Invoke();
        }
        // CASE: OBJECT WAS JUST RELEASED
        else if (previousValue.TryGet(out NetworkObject oldNetObj))
        {
            Vector3 dropPosition = transform.position + (transform.forward * grabDistance);

            if (oldNetObj.TryGetComponent<Unity.Netcode.Components.NetworkTransform>(out var netTransform))
            {
                netTransform.enabled = false;

                oldNetObj.transform.position = dropPosition;
                oldNetObj.transform.rotation = Quaternion.identity;

                var navMeshAgent = oldNetObj.GetComponent<NavMeshAgent>();
                navMeshAgent.Warp(dropPosition);
                
                netTransform.enabled = true;
            }
            else
            {
                var navMeshAgent = oldNetObj.GetComponent<NavMeshAgent>();
                navMeshAgent.Warp(dropPosition);
            }
            OnRelease?.Invoke();
            UpdateVisuals(oldNetObj.GetComponent<Grabbable>(), false);
        }
        else
        {

            OnRelease?.Invoke();
            UpdateVisuals(false);
        }
    }

    private void UpdateVisuals(Grabbable grabbable, bool isGrabbing)
    {
        if (isGrabbing)
        {
            if (grabbable != null)
            {
                visualModel = Instantiate(grabbable.GrabbedObjectModel, handPoint.transform, false);
                visualModel.transform.localPosition = Vector3.zero;
                visualModel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                grabbable.SetGrabbed(true); // Usually hides the mesh/disables physics
            }
        }
        else
        {
            if (visualModel != null) Destroy(visualModel);
            if (grabbable != null) grabbable.SetGrabbed(false); // Shows the mesh/enables physics
        }
    }

    private void UpdateVisuals(bool isGrabbing)
    {
        if (visualModel != null) Destroy(visualModel);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, grabRadius);
        Vector3 endPoint = transform.position + (transform.forward * grabDistance);
        Gizmos.DrawLine(transform.position, endPoint);
        Gizmos.DrawWireSphere(endPoint, grabRadius);
    }
}