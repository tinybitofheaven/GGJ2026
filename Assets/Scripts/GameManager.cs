using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    private void SetInstance()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

    }
    
    [SerializeField] private MaskDatabase maskDatabase;

    private List<NetworkObject> connectedPlayers = new ();
    
    private PlayerData bouncer;
    private PlayerData security;
    
    public event Action OnGameStart;
    public event Action OnGameEnd;
    public event Action OnImagesSelected;
    public event Action<int> OnCurrentImageChanged;
    
    public NetworkList<int> selectedImageIds;
    
    private NetworkVariable<int> currentImageIndex = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public int CurrentImageIndex => currentImageIndex.Value;
    
    private void Awake()
    {
        SetInstance();
        
        selectedImageIds = new NetworkList<int>();
    }

    private void Update()
    {
        if (security != null && security.NetworkObject.IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                PreviousImage();
            }
        
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                NextImage();
            }
        }
    }

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;
        }
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        selectedImageIds.OnListChanged += OnSelectedImagesChanged;
        currentImageIndex.OnValueChanged += OnCurrentImageIndexChanged;
    }
    
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        selectedImageIds.OnListChanged -= OnSelectedImagesChanged;
        currentImageIndex.OnValueChanged -= OnCurrentImageIndexChanged;
    }
    
    private void OnSelectedImagesChanged(NetworkListEvent<int> changeEvent)
    {
        Debug.Log($"Selected images changed: {changeEvent.Type}");
        
        if (selectedImageIds.Count == 3)
        {
            OnImagesSelected?.Invoke();
        }
    }
    
    private void OnCurrentImageIndexChanged(int oldValue, int newValue)
    {
        Debug.Log($"Current image changed from {oldValue} to {newValue}");
        OnCurrentImageChanged?.Invoke(newValue);
    }
    
    private void OnConnectionEvent(NetworkManager manager, ConnectionEventData eventData)
    {
        Debug.Log("OnConnectionEvent: " + eventData);
        if (NetworkManager.Singleton.IsHost && GetPlayerCount() == 2)
        {
            SelectRandomImages();
            StartGame();
        }
    }

    private void StartGame()
    {
        connectedPlayers.Clear();

        foreach (KeyValuePair<ulong, NetworkObject> kvp in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
        {
            // Check if the object is a player object
            if (kvp.Value.IsPlayerObject)
            {
                connectedPlayers.Add(kvp.Value);
            }
        }
        
        var player1 = connectedPlayers.First().GetComponent<PlayerData>();
        var player2 = connectedPlayers.Last().GetComponent<PlayerData>();

        if (player1.IsHost)
        {
            bouncer = player2;
            security = player1;
        }
        
        // if (Random.value < 0.5f)
        // {
        //     bouncer = connectedPlayers.First().GetComponent<PlayerData>();
        //     security = connectedPlayers.Last().GetComponent<PlayerData>();
        // }
        // else
        // {
        //     security = connectedPlayers.First().GetComponent<PlayerData>();
        //     bouncer = connectedPlayers.Last().GetComponent<PlayerData>();
        // }
            
        bouncer.SetRole(Role.Bouncer);
        security.SetRole(Role.Security);

        NotifyGameStartClientRpc();
    }
    
    private void SelectRandomImages()
    {
        if (!IsServer) return;
        
        // Clear previous selection
        selectedImageIds.Clear();
        
        // Get total number of images
        int totalImages = maskDatabase.allImages.Length;
        
        if (totalImages < 3)
        {
            Debug.LogError("Not enough images in database! Need at least 3.");
            return;
        }
        
        // Create a list of available indices
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < totalImages; i++)
        {
            availableIndices.Add(i);
        }
        
        // Select 3 random images
        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, availableIndices.Count);
            int selectedId = availableIndices[randomIndex];
            
            selectedImageIds.Add(selectedId);
            availableIndices.RemoveAt(randomIndex);
            
            Debug.Log($"Selected image ID: {selectedId}");
        }
        
        // Reset to first image
        currentImageIndex.Value = 0;
    }
    
    // Server methods to change current image
    public void NextImage()
    {
        if (!IsServer)
        {
            NextImageServerRpc();
            return;
        }
        
        if (currentImageIndex.Value < selectedImageIds.Count - 1)
        {
            currentImageIndex.Value++;
        }
        else
        {
            Debug.Log("Already at last image");
        }
    }
    
    public void PreviousImage()
    {
        if (!IsServer)
        {
            PreviousImageServerRpc();
            return;
        }
        
        if (currentImageIndex.Value > 0)
        {
            currentImageIndex.Value--;
        }
        else
        {
            Debug.Log("Already at first image");
        }
    }
    
    // ServerRpcs for clients to request image changes
    [ServerRpc(RequireOwnership = false)]
    private void NextImageServerRpc()
    {
        NextImage();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void PreviousImageServerRpc()
    {
        PreviousImage();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetCurrentImageServerRpc(int index)
    {
        SetCurrentImage(index);
    }
    
    // Helper methods to get sprites
    public List<Sprite> GetSelectedSprites()
    {
        List<Sprite> sprites = new List<Sprite>();
        
        foreach (int id in selectedImageIds)
        {
            Sprite sprite = maskDatabase.GetSpriteById(id);
            if (sprite != null)
            {
                sprites.Add(sprite);
            }
        }
        
        return sprites;
    }
    
    public Sprite GetCurrentSprite()
    {
        if (selectedImageIds.Count == 0)
        {
            Debug.LogWarning("No images selected yet!");
            return null;
        }
        
        if (currentImageIndex.Value < 0 || currentImageIndex.Value >= selectedImageIds.Count)
        {
            Debug.LogWarning($"Invalid current image index: {currentImageIndex.Value}");
            return null;
        }
        
        int imageId = selectedImageIds[currentImageIndex.Value];
        return maskDatabase.GetSpriteById(imageId);
    }
    
    public int GetCurrentImageId()
    {
        if (currentImageIndex.Value >= 0 && currentImageIndex.Value < selectedImageIds.Count)
        {
            return selectedImageIds[currentImageIndex.Value];
        }
        
        return -1;
    }
    
    public void SetCurrentImage(int index)
    {
        if (!IsServer)
        {
            SetCurrentImageServerRpc(index);
            return;
        }
        
        if (index >= 0 && index < selectedImageIds.Count)
        {
            currentImageIndex.Value = index;
        }
        else
        {
            Debug.LogWarning($"Invalid image index: {index}");
        }
    }
    
    [ClientRpc]
    private void NotifyGameStartClientRpc()
    {
        OnGameStart?.Invoke();
    }

    public int GetPlayerCount()
    {
        // This will be accurate on the Host/Server.
        // Clients only see 1 (their own connection) unless synced otherwise.
        return NetworkManager.Singleton.ConnectedClientsList.Count;
    }
}
