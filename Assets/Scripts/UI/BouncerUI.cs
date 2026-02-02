using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BouncerUI : NetworkBehaviour
{
    public GameObject maskInspectorUI;
    public Image guestCardMaskImage;
    public Image maskInspectorMaskImage;

    private bool isMaskInspectorOpen = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (isMaskInspectorOpen)
            {
                maskInspectorUI.SetActive(false);
                isMaskInspectorOpen = false;
            }
            else
            {
                maskInspectorUI.SetActive(true);
                isMaskInspectorOpen = true;
            }
        }
    }

    public void SetBouncerUICallbacks(PlayerData playerObj)
    {
        maskInspectorUI.SetActive(false);

        playerObj.playerGrab.OnGrab += DisplayMask;
        playerObj.playerGrab.OnRelease += DisplayMask;
        
        guestCardMaskImage.enabled = false;
        maskInspectorMaskImage.enabled = false;
    }

    
    public void RemoveBouncerUICallbacks(PlayerData playerObj)
    {
        playerObj.playerGrab.OnGrab -= DisplayMask;
        playerObj.playerGrab.OnRelease -= DisplayMask;
    }
    
    private void DisplayMask()
    {
        foreach (var playerObj in FindObjectsOfType<PlayerData>())
        {
            if (playerObj.IsOwner)
            {
                if (playerObj.IsBouncer())
                {
                    if (playerObj.playerGrab.GrabbedObject.TryGet(out NetworkObject netObj))
                    {
                        Sprite maskSprite = GameManager.Instance.MaskDatabase.GetSpriteById(netObj.GetComponent<NPCData>().GetMaskId());
                        maskInspectorMaskImage.sprite = maskSprite;
                        maskInspectorMaskImage.enabled = true;
                        guestCardMaskImage.sprite = maskSprite;
                        guestCardMaskImage.enabled = true;
                    }
                    else
                    {
                        guestCardMaskImage.enabled = false;
                        maskInspectorMaskImage.enabled = false;
                    }
                }
            }
        }
    }
}
