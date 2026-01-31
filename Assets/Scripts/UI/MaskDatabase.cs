using UnityEngine;

[CreateAssetMenu(fileName = "MaskDatabase", menuName = "Game/Image Database")]
public class MaskDatabase : ScriptableObject
{
    [System.Serializable]
    public class ImageData
    {
        public int id;
        public Sprite sprite;
        public string imageName;
    }
    
    public ImageData[] allImages;
    
    public Sprite GetSpriteById(int id)
    {
        foreach (var img in allImages)
        {
            if (img.id == id)
                return img.sprite;
        }
        
        Debug.LogWarning($"Image with ID {id} not found!");
        return null;
    }
    
    public ImageData GetImageById(int id)
    {
        foreach (var img in allImages)
        {
            if (img.id == id)
                return img;
        }
        
        return null;
    }

    public int GetImageCount()
    {
        return allImages.Length;
    }
}