using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CanvasData : MonoBehaviour
{
    [SerializeField] private SecurityUI securityGuardUI; 
    [SerializeField] private BouncerUI bouncerUI; 
    [SerializeField] private MainMenu mainMenu;
    
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
        mainMenu.DisableAllMenus();
        mainMenu.GetComponent<Image>().enabled = false;
        ShowRoleSpecificUI();
    }
    
    private void ShowRoleSpecificUI()
    {
        foreach (var playerObj in FindObjectsOfType<PlayerData>())
        {
            if (playerObj.IsOwner)
            {
                if (playerObj.IsSecurity())
                {
                    securityGuardUI.gameObject.SetActive(true);
                    bouncerUI.gameObject.SetActive(false);
                    bouncerUI.RemoveBouncerUICallbacks(playerObj);

                }
                else
                {
                    securityGuardUI.gameObject.SetActive(false);
                    bouncerUI.gameObject.SetActive(true);
                    bouncerUI.SetBouncerUICallbacks(playerObj);
                }
            }
        }
    }
}
