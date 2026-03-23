using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Main lobby UI - handles room creation, joining, and table selection
/// </summary>
public class LobbyUI : MonoBehaviourPunCallbacks
{
    public static LobbyUI Instance;

    [Header("Player Info Panel")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text playerBalanceText;
    [SerializeField] private Image playerAvatar;

    [Header("Room List")]
    [SerializeField] private Transform roomListContainer;
    [SerializeField] private GameObject roomEntryPrefab;
    [SerializeField] private GameObject noRoomsMessage;

    [Header("Create Room Panel")]
    [SerializeField] private GameObject createRoomPanel;
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private Button[] tableStakesButtons; // Low, Medium, High
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button cancelButton;

    [Header("Quick Join")]
    [SerializeField] private Button[] quickJoinButtons; // Low, Medium, High
    [SerializeField] private Button randomJoinButton;

    [Header("Loading")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text loadingText;

    [Header("Settings")]
    [SerializeField] private string rouletteGameScene = "RouletteGame";

    private TableStakes selectedTableStakes = TableStakes.Medium;
    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Setup button listeners
        SetupButtons();

        // Update player info
        UpdatePlayerInfo();

        // Hide create room panel initially
        if (createRoomPanel != null)
            createRoomPanel.SetActive(false);

        // Hide loading panel
        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        // Set player name
        if (!string.IsNullOrEmpty(PhotonNetwork.NickName) && playerNameText != null)
        {
            playerNameText.text = PhotonNetwork.NickName;
        }
    }

    private void SetupButtons()
    {
        // Create room button
        if (createRoomButton != null)
            createRoomButton.onClick.AddListener(OnCreateRoomClicked);

        // Cancel button
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);

        // Table stakes buttons
        for (int i = 0; i < tableStakesButtons.Length; i++)
        {
            int stakesIndex = i;
            tableStakesButtons[i].onClick.AddListener(() => OnTableStakesSelected(stakesIndex));
        }

        // Quick join buttons
        for (int i = 0; i < quickJoinButtons.Length; i++)
        {
            int stakesIndex = i;
            quickJoinButtons[i].onClick.AddListener(() => OnQuickJoinClicked((TableStakes)stakesIndex));
        }

        // Random join
        if (randomJoinButton != null)
            randomJoinButton.onClick.AddListener(OnRandomJoinClicked);
    }

    #region UI Updates

    private void UpdatePlayerInfo()
    {
        if (playerNameText != null)
        {
            playerNameText.text = PhotonNetwork.NickName;
        }

        if (playerBalanceText != null)
        {
            // This would come from player data
            playerBalanceText.text = "$3,000";
        }
    }

    private void UpdateRoomList()
    {
        // Clear existing entries
        foreach (Transform child in roomListContainer)
        {
            Destroy(child.gameObject);
        }

        // Show no rooms message if empty
        if (noRoomsMessage != null)
        {
            noRoomsMessage.SetActive(cachedRoomList.Count == 0);
        }

        // Create room entries
        foreach (RoomInfo room in cachedRoomList)
        {
            CreateRoomEntry(room);
        }
    }

    private void CreateRoomEntry(RoomInfo room)
    {
        if (roomEntryPrefab == null || roomListContainer == null) return;

        GameObject entry = Instantiate(roomEntryPrefab, roomListContainer);

        // Setup entry UI
        RoomListEntry entryScript = entry.GetComponent<RoomListEntry>();
        if (entryScript != null)
        {
            entryScript.Setup(room, OnJoinRoomClicked);
        }
    }

    #endregion

    #region Button Handlers

    /// <summary>
    /// Show create room panel
    /// </summary>
    public void OnShowCreateRoom()
    {
        if (createRoomPanel != null)
            createRoomPanel.SetActive(true);

        // Set default room name
        if (roomNameInput != null && string.IsNullOrEmpty(roomNameInput.text))
        {
            roomNameInput.text = $"Room_{Random.Range(1000, 9999)}";
        }
    }

    /// <summary>
    /// Create room button clicked
    /// </summary>
    private void OnCreateRoomClicked()
    {
        string roomName = roomNameInput.text;

        if (string.IsNullOrEmpty(roomName))
        {
            roomName = $"Room_{Random.Range(1000, 9999)}";
        }

        ShowLoading("Creating Room...");

        NetworkManager.Instance.CreateRoom(roomName, selectedTableStakes);
    }

    /// <summary>
    /// Cancel button clicked
    /// </summary>
    private void OnCancelClicked()
    {
        if (createRoomPanel != null)
            createRoomPanel.SetActive(false);
    }

    /// <summary>
    /// Table stakes selection
    /// </summary>
    private void OnTableStakesSelected(int index)
    {
        selectedTableStakes = (TableStakes)index;

        // Update button visuals
        for (int i = 0; i < tableStakesButtons.Length; i++)
        {
            ColorBlock colors = tableStakesButtons[i].colors;
            colors.normalColor = (i == index) ? Color.green : Color.white;
            tableStakesButtons[i].colors = colors;
        }
    }

    /// <summary>
    /// Quick join specific table type
    /// </summary>
    private void OnQuickJoinClicked(TableStakes stakes)
    {
        ShowLoading($"Finding {stakes} Stakes Table...");

        NetworkManager.Instance.JoinRandomRoom(stakes);
    }

    /// <summary>
    /// Random join any table
    /// </summary>
    private void OnRandomJoinClicked()
    {
        ShowLoading("Finding Available Table...");

        NetworkManager.Instance.JoinRandomRoom();
    }

    /// <summary>
    /// Join specific room from list
    /// </summary>
    private void OnJoinRoomClicked(string roomName)
    {
        ShowLoading($"Joining {roomName}...");

        NetworkManager.Instance.JoinRoom(roomName);
    }

    #endregion

    #region Loading

    private void ShowLoading(string message)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            if (loadingText != null)
                loadingText.text = message;
        }
    }

    private void HideLoading()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    #endregion

    #region Photon Callbacks

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log($"[LobbyUI] Room list updated: {roomList.Count} rooms");

        // Update cached list
        foreach (RoomInfo room in roomList)
        {
            // Remove from list if room is removed
            if (!room.IsVisible || room.RemovedFromList)
            {
                cachedRoomList.RemoveAll(r => r.Name == room.Name);
            }
            // Add or update room
            else
            {
                int index = cachedRoomList.FindIndex(r => r.Name == room.Name);
                if (index >= 0)
                {
                    cachedRoomList[index] = room;
                }
                else
                {
                    cachedRoomList.Add(room);
                }
            }
        }

        // Update UI
        UpdateRoomList();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("[LobbyUI] Joined lobby");
        HideLoading();
        cachedRoomList.Clear();
        UpdateRoomList();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[LobbyUI] Joined room: {PhotonNetwork.CurrentRoom.Name}");
        HideLoading();

        // The NetworkManager will handle scene loading
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[LobbyUI] Join random failed: {message}");
        HideLoading();

        // Show error - could create room instead
        ShowLoading("No tables available. Creating new room...");
        NetworkManager.Instance.CreateRoom(null, TableStakes.Medium);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        HideLoading();
        Debug.LogWarning($"[LobbyUI] Disconnected: {cause}");

        // Return to connection screen
        UnityEngine.SceneManagement.SceneManager.LoadScene("Launcher");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Set player name
    /// </summary>
    public void SetPlayerName(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            PhotonNetwork.NickName = name;
            if (playerNameText != null)
                playerNameText.text = name;
        }
    }

    #endregion
}
