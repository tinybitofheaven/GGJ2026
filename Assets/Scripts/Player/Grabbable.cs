using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class Grabbable : NetworkBehaviour
{
    private NetworkObject networkObject;
    public MonoBehaviour[] behavioursToDisable;

    public NetworkObject NetworkObject { get => networkObject; }
    public GameObject GrabbedObjectModel;
    private Collider grabCollider;
    
    private void Start()
    {
        networkObject = GetComponent<NetworkObject>();
        grabCollider = GetComponent<Collider>();
    }

    public void SetGrabbed(bool grabbed)
    {
        foreach (MonoBehaviour behaviour in behavioursToDisable)
        {
            // if grabbed, disable everything
            behaviour.enabled = !grabbed;
        }
        GrabbedObjectModel.SetActive(!grabbed);
        grabCollider.enabled = !grabbed;
    }
}
