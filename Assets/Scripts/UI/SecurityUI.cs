using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SecurityUI : MonoBehaviour
{
    [SerializeField] private Image maskImage;

    private void Start()
    {
        gameObject.SetActive(false);
    }
    
    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMaskChanged += DisplayMask;

            DisplayMask();
        }
    }
    
    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMaskChanged -= DisplayMask;
        }
    }
    
    private void DisplayMask(int index = 0)
    {
        Sprite currentSprite = GameManager.Instance.GetCurrentSprite();
        
        if (currentSprite != null && maskImage != null)
        {
            maskImage.sprite = currentSprite;
            Debug.Log($"Displaying current image: {index}");
        }
    }
}
