using System;
using Unity.Netcode;
using UnityEngine;

public class CanvasData : MonoBehaviour
{
    [SerializeField] private GameObject multiplayerMenu;
    [SerializeField] private SecurityUI securityGuardUI;  
    
    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart += HandleGameStart;
        }
    }
    
    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart -= HandleGameStart;
        }
    }
    
    private void HandleGameStart()
    {
        DisableMultiplayerMenu();
        ShowRoleSpecificUI();
    }
    
    private void ShowRoleSpecificUI()
    {
        // Find the local player
        foreach (var playerObj in FindObjectsOfType<PlayerData>())
        {
            Debug.Log($"ShowRoleSpecificUI {playerObj}");
            // Check if this is the local player (the one we control)
            if (playerObj.IsOwner)
            {
                if (playerObj.IsSecurity())
                {
                    securityGuardUI.gameObject.SetActive(true);
                    Debug.Log("Showing security UI");
                }
                else
                {
                    securityGuardUI.gameObject.SetActive(false);
                    Debug.Log("Hiding security UI - player is bouncer");
                }
                break;  // Found our player, stop searching
            }
        }
    }

    private void DisableMultiplayerMenu()
    {
        multiplayerMenu.SetActive(false);
    }
}
