using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class SecurityUI : MonoBehaviour
{
    [SerializeField] private Image targetMask;
    private Color transparent = new Color(0f, 0f, 0f, 0f);
    private Color grey = new Color(0.4f, 0.4f, 0.4f, 1f);
    private Color green = new Color(0, 1f, 0, 1f);
    private Color red = new Color(1, 0, 0, 1f);
    public Image[] guestEjectedIcons;
    public Image[] checkmarkIcons;
    public Image[] errorIcons;
    
    private int numberEjected = 0;
    
    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            Debug.Log($"SecurityUI: OnEnable");
            GameManager.Instance.OnMaskChanged += DisplayMask;
            GameManager.Instance.EjectionData.OnValueChanged += RefreshUI;

            DisplayMask(0,0);
            RefreshUI(new EjectionStats(), GameManager.Instance.EjectionData.Value);
        }

        foreach (Image guestEjected in guestEjectedIcons)
        {
            guestEjected.color = transparent;
        }
    }
    
    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMaskChanged -= DisplayMask;
            GameManager.Instance.EjectionData.OnValueChanged -= RefreshUI;
        }
    }
    
    private void DisplayMask(int oldValue, int newValue)
    {
        Debug.Log($"DisplayMask {newValue}");
        Sprite currentSprite = GameManager.Instance.GetCurrentMask();
        
        if (currentSprite != null && targetMask != null)
        {
            targetMask.sprite = currentSprite;
        }
    }

    private void RefreshUI(EjectionStats previous, EjectionStats current)
    {
        if (current.Total - 1 == -1) return;

        bool correctGuess = current.Correct > previous.Correct;
        
        guestEjectedIcons[current.Total - 1].color = correctGuess ? green : red;
        guestEjectedIcons[current.Total - 1].sprite = targetMask.sprite;
        
        if (correctGuess)
        {
            checkmarkIcons[current.Correct - 1].color = green;
        }
        else
        {
            errorIcons[current.Incorrect - 1].color = red;
        }

        // DisplayMask(0, 0);
    }
}
