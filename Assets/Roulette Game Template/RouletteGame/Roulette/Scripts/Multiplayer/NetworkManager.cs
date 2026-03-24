using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

/// <summary>
/// Main Photon PUN2 Network Manager - Handles connection, room operations, and game flow
/// </summary>
public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    [Header("Game Settings")]
    [SerializeField] private string gameVersion = "1.0";
    [SerializeField] private byte maxPlayersPerRoom = 4;
    [SerializeField] private string lobbyScene = "Lobby";
    [SerializeField] private string rouletteScene = "RouletteGame";
    [SerializeField] private bool useSingleSceneMode = true; // Use everything in one scene

    [Header("Player Settings")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Debug")]
    [SerializeField] private bool isDebugMode = false;

    // Events
    public System.Action OnConnectedToPhotonEvent;
    public System.Action OnDisconnectedFromPhotonEvent;
    public System.Action OnJoinedLobbyEvent;
    public System.Action OnJoinedRoomEvent;
    public System.Action OnLeftRoomEvent;
    public System.Action<string> OnConnectionFailedEvent;

    // Properties
    public bool IsConnected => PhotonNetwork.IsConnected;
    public bool InRoom => PhotonNetwork.InRoom;
    public bool IsMasterClient => PhotonNetwork.IsMasterClient;
    public int PlayerCount => PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;
    public int MaxPlayers => maxPlayersPerRoom;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Photon settings
        PhotonNetwork.AutomaticallySyncScene = !useSingleSceneMode;
    }

    private void Start()
    {
        // Auto-connect on start if using single scene mode for immediate gameplay test
        if ((isDebugMode || useSingleSceneMode) && !PhotonNetwork.IsConnected)
        {
            ConnectToPhoton();
        }
    }

    #region Connection Methods

    /// <summary>
    /// Connect to Photon servers
    /// </summary>
    public void ConnectToPhoton()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("[NetworkManager] Already connected to Photon");
            return;
        }

        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            SetPlayerNickname($"Guest_{Random.Range(100, 999)}");
        }

        Debug.Log("[NetworkManager] Connecting to Photon...");
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// Disconnect from Photon
    /// </summary>
    public void DisconnectFromPhoton()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("[NetworkManager] Disconnecting from Photon...");
            PhotonNetwork.Disconnect();
        }
    }

    #endregion

    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("[NetworkManager] Connected to Master Server");
        
        if (useSingleSceneMode)
        {
            Debug.Log("[NetworkManager] Waiting for player to select Quick Play or enter an Invite Code...");
            // UI Toolkit Main Menu Panel will handle JoinRandomRoom() or JoinRoom()
        }
        else
        {
            // Join the lobby to see available rooms
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("[NetworkManager] Joined Lobby");
        OnJoinedLobbyEvent?.Invoke();
        
        // Load lobby scene if not already there and we are not in single scene mode
        if (!useSingleSceneMode && SceneManager.GetActiveScene().name != lobbyScene)
        {
            SceneManager.LoadScene(lobbyScene);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"[NetworkManager] Disconnected: {cause}");
        OnDisconnectedFromPhotonEvent?.Invoke();
    }

    public override void OnConnected()
    {
        Debug.Log("[NetworkManager] Connected to Photon");
        OnConnectedToPhotonEvent?.Invoke();
    }

    #endregion

    #region Room Methods

    /// <summary>
    /// Create a new room with custom properties (table stakes)
    /// </summary>
    public void CreateRoom(string roomName, TableStakes tableStakes)
    {
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = GenerateRoomName();
        }

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = maxPlayersPerRoom,
            IsVisible = true,
            IsOpen = true,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "TableStakes", (int)tableStakes },
                { "MinBet", GetMinBetForStakes(tableStakes) },
                { "MaxBet", GetMaxBetForStakes(tableStakes) },
                { "GamePhase", (int)GamePhase.Waiting }
            },
            CustomRoomPropertiesForLobby = new string[] { "TableStakes", "MinBet", "MaxBet" }
        };

        Debug.Log($"[NetworkManager] Creating room: {roomName} with stakes: {tableStakes}");
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    /// <summary>
    /// Join an existing room by name
    /// </summary>
    public void JoinRoom(string roomName)
    {
        Debug.Log($"[NetworkManager] Joining room: {roomName}");
        PhotonNetwork.JoinRoom(roomName);
    }

    /// <summary>
    /// Join a random available room
    /// </summary>
    public void JoinRandomRoom()
    {
        Debug.Log("[NetworkManager] Joining random room...");
        PhotonNetwork.JoinRandomRoom();
    }

    /// <summary>
    /// Join a random room with specific table stakes
    /// </summary>
    public void JoinRandomRoom(TableStakes tableStakes)
    {
        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "TableStakes", (int)tableStakes }
        };
        PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 0);
    }

    /// <summary>
    /// Leave the current room
    /// </summary>
    public void LeaveRoom()
    {
        Debug.Log("[NetworkManager] Leaving room");
        PhotonNetwork.LeaveRoom();
    }

    #endregion

    #region Room Callbacks

    public override void OnCreatedRoom()
    {
        Debug.Log("[NetworkManager] Room created successfully");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[NetworkManager] Joined room: {PhotonNetwork.CurrentRoom.Name}");
        Debug.Log($"[NetworkManager] Players in room: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}");
        
        // Spawn the player prefab so they can place bets
        if (playerPrefab != null)
        {
            PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
        }
        else
        {
            PhotonNetwork.Instantiate("NetworkPlayer", Vector3.zero, Quaternion.identity);
        }

        OnJoinedRoomEvent?.Invoke();

        // Load the game scene if not in single scene mode
        if (!useSingleSceneMode && SceneManager.GetActiveScene().name != rouletteScene)
        {
            PhotonNetwork.LoadLevel(rouletteScene);
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("[NetworkManager] Left room");
        OnLeftRoomEvent?.Invoke();
        
        // Return to lobby if not in single scene mode
        if (!useSingleSceneMode)
        {
            SceneManager.LoadScene(lobbyScene);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[NetworkManager] Create room failed: {message}");
        OnConnectionFailedEvent?.Invoke($"Failed to create room: {message}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[NetworkManager] Join room failed: {message}");
        OnConnectionFailedEvent?.Invoke($"Failed to join room: {message}");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[NetworkManager] Join random failed: {message}");
        
        // Create a new room if no rooms available
        Debug.Log("[NetworkManager] Creating new room since none available");
        CreateRoom(null, TableStakes.Medium);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"[NetworkManager] Player entered: {newPlayer.NickName}");
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log($"[NetworkManager] Player left: {otherPlayer.NickName}");
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        Debug.Log($"[NetworkManager] Master client switched to: {newMasterClient.NickName}");
    }

    #endregion

    #region Helper Methods

    private string GenerateRoomName()
    {
        return Random.Range(1000, 9999).ToString();
    }

    private int GetMinBetForStakes(TableStakes stakes)
    {
        return stakes switch
        {
            TableStakes.Low => 1,
            TableStakes.Medium => 10,
            TableStakes.High => 100,
            _ => 10
        };
    }

    private int GetMaxBetForStakes(TableStakes stakes)
    {
        return stakes switch
        {
            TableStakes.Low => 200,
            TableStakes.Medium => 1000,
            TableStakes.High => 10000,
            _ => 1000
        };
    }

    /// <summary>
    /// Get current room's table stakes
    /// </summary>
    public TableStakes GetTableStakes()
    {
        if (PhotonNetwork.CurrentRoom == null) return TableStakes.Medium;
        
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("TableStakes", out object stakes))
        {
            return (TableStakes)(int)stakes;
        }
        return TableStakes.Medium;
    }

    /// <summary>
    /// Get current room's min bet
    /// </summary>
    public int GetMinBet()
    {
        if (PhotonNetwork.CurrentRoom == null) return 10;
        
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("MinBet", out object minBet))
        {
            return (int)minBet;
        }
        return 10;
    }

    /// <summary>
    /// Get current room's max bet
    /// </summary>
    public int GetMaxBet()
    {
        if (PhotonNetwork.CurrentRoom == null) return 1000;
        
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("MaxBet", out object maxBet))
        {
            return (int)maxBet;
        }
        return 1000;
    }

    #endregion

    #region Player Methods

    /// <summary>
    /// Set player nickname
    /// </summary>
    public void SetPlayerNickname(string nickname)
    {
        if (string.IsNullOrEmpty(nickname))
        {
            nickname = $"Player_{Random.Range(1000, 9999)}";
        }
        PhotonNetwork.NickName = nickname;
    }

    /// <summary>
    /// Get all players in room
    /// </summary>
    public System.Collections.Generic.List<Photon.Realtime.Player> GetPlayersInRoom()
    {
        return new System.Collections.Generic.List<Photon.Realtime.Player>(PhotonNetwork.CurrentRoom.Players.Values);
    }

    #endregion
}

/// <summary>
/// Table stakes levels
/// </summary>
public enum TableStakes
{
    Low = 0,
    Medium = 1,
    High = 2
}

/// <summary>
/// Game phases for synchronization
/// </summary>
public enum GamePhase
{
    Waiting,        // Waiting for players
    Betting,        // Players placing bets
    Spinning,       // Wheel spinning
    Result,         // Showing result
    Payout          // Distributing winnings
}
