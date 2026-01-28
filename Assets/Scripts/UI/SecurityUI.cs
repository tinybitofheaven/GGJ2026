using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SecurityUI : NetworkBehaviour
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
            GameManager.Instance.OnCurrentImageChanged += DisplayCurrentImage;
        }
    }
    
    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCurrentImageChanged -= DisplayCurrentImage;
        }
    }
    
    private void DisplayCurrentImage(int index)
    {
        Sprite currentSprite = GameManager.Instance.GetCurrentSprite();
        
        if (currentSprite != null && maskImage != null)
        {
            maskImage.sprite = currentSprite;
            Debug.Log($"Displaying current image: {index}");
        }
    }
}
