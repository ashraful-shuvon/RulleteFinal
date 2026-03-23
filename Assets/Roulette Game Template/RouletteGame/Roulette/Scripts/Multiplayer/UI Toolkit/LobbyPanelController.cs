using UnityEngine;
using UnityEngine.UIElements;
using Photon.Pun;
using System.Collections.Generic;

/// <summary>
/// Controller for the in-game multiplayer panel (invite/join system)
/// Uses UI Toolkit (UXML/USS)
/// </summary>
public class LobbyPanelController : MonoBehaviour
{
    [Header("UI Toolkit References")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset playerEntryAsset;
    [SerializeField] private VisualTreeAsset tableEntryAsset;

    // Panel references
    private VisualElement lobbyPanel;
    private Label connectionLabel;
    private Label pingLabel;
    private Label roomCodeText;
    private Label tableNameLabel;
    private DropdownField stakesDropdown;
    private IntegerField minBetInput;
    private IntegerField maxBetInput;
    private IntegerField startBalanceInput;
    private TextField joinCodeInput;
    private ListView playersListView;
    private ListView availableTablesListView;
    private Label playerCountLabel;

    // Buttons
    private Button closeButton;
    private Button copyCodeButton;
    private Button inviteButton;
    private Button shareButton;
    private Button joinByCodeButton;
    private Button quickJoinButton;
    private Button createPrivateButton;
    private Button leaveButton;
    private Button startGameButton;
    private Button addBotButton;

    // State
    private List<PlayerInfo> playersInRoom = new List<PlayerInfo>();
    private List<TableInfo> availableTables = new List<TableInfo>();

    private void Awake()
    {
        // Get UI Document
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();
    }

    private void Start()
    {
        SetupUI();
        SetupButtonCallbacks();
        UpdateUI();

        // Subscribing to networked events to act as an in-game overlay
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnJoinedRoomEvent += OnJoinedRoomEvent;
            NetworkManager.Instance.OnLeftRoomEvent += ShowPanel;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnJoinedRoomEvent -= OnJoinedRoomEvent;
            NetworkManager.Instance.OnLeftRoomEvent -= ShowPanel;
        }
    }

    private void OnJoinedRoomEvent()
    {
        // Automatically hide the Lobby overlay when successfully joining a table
        HidePanel();
    }

    private void SetupUI()
    {
        var root = uiDocument.rootVisualElement;

        // Main panel
        lobbyPanel = root.Q<VisualElement>("LobbyPanel");

        // Header
        connectionLabel = root.Q<Label>("ConnectionLabel");
        pingLabel = root.Q<Label>("PingLabel");

        // Room info
        roomCodeText = root.Q<Label>("RoomCodeText");
        tableNameLabel = root.Q<Label>("TableNameLabel");

        // Settings
        stakesDropdown = root.Q<DropdownField>("StakesDropdown");
        minBetInput = root.Q<IntegerField>("MinBetInput");
        maxBetInput = root.Q<IntegerField>("MaxBetInput");
        startBalanceInput = root.Q<IntegerField>("StartBalanceInput");

        // Join section
        joinCodeInput = root.Q<TextField>("JoinCodeInput");

        // Lists
        playersListView = root.Q<ListView>("PlayersListView");
        availableTablesListView = root.Q<ListView>("AvailableTablesListView");
        playerCountLabel = root.Q<Label>("PlayerCountLabel");

        // Buttons
        closeButton = root.Q<Button>("CloseButton");
        copyCodeButton = root.Q<Button>("CopyCodeButton");
        inviteButton = root.Q<Button>("InviteButton");
        shareButton = root.Q<Button>("ShareButton");
        joinByCodeButton = root.Q<Button>("JoinByCodeButton");
        quickJoinButton = root.Q<Button>("QuickJoinButton");
        createPrivateButton = root.Q<Button>("CreatePrivateButton");
        leaveButton = root.Q<Button>("LeaveButton");
        startGameButton = root.Q<Button>("StartGameButton");
        addBotButton = root.Q<Button>("AddBotButton");

        // Setup list views
        if (playersListView != null)
        {
            playersListView.makeItem = () => playerEntryAsset.CloneTree();
            playersListView.bindItem = BindPlayerItem;
            playersListView.itemsSource = playersInRoom;
        }

        if (availableTablesListView != null)
        {
            availableTablesListView.makeItem = () => tableEntryAsset.CloneTree();
            availableTablesListView.bindItem = BindTableItem;
            availableTablesListView.itemsSource = availableTables;
        }

        // Hide panel initially
        HidePanel();
    }

    private void SetupButtonCallbacks()
    {
        closeButton?.RegisterCallback<ClickEvent>(evt => HidePanel());
        copyCodeButton?.RegisterCallback<ClickEvent>(evt => CopyRoomCode());
        inviteButton?.RegisterCallback<ClickEvent>(evt => InvitePlayer());
        shareButton?.RegisterCallback<ClickEvent>(evt => ShareRoomCode());
        joinByCodeButton?.RegisterCallback<ClickEvent>(evt => JoinByCode());
        quickJoinButton?.RegisterCallback<ClickEvent>(evt => QuickJoin());
        createPrivateButton?.RegisterCallback<ClickEvent>(evt => CreatePrivateRoom());
        leaveButton?.RegisterCallback<ClickEvent>(evt => LeaveRoom());
        startGameButton?.RegisterCallback<ClickEvent>(evt => StartGame());
        addBotButton?.RegisterCallback<ClickEvent>(evt => AddBot());
    }

    #region Panel Control

    public void ShowPanel()
    {
        lobbyPanel?.SetEnabled(true);
        lobbyPanel?.RemoveFromClassList("hidden");
        UpdateUI();
    }

    public void HidePanel()
    {
        lobbyPanel?.AddToClassList("hidden");
        lobbyPanel?.SetEnabled(false);
    }

    public void TogglePanel()
    {
        if (lobbyPanel?.ClassListContains("hidden") ?? true)
            ShowPanel();
        else
            HidePanel();
    }

    #endregion

    #region UI Updates

    private void UpdateUI()
    {
        UpdateConnectionStatus();
        UpdateRoomInfo();
        UpdatePlayerList();
    }

    private void UpdateConnectionStatus()
    {
        if (connectionLabel == null) return;

        if (PhotonNetwork.IsConnected)
        {
            connectionLabel.text = "● Connected";
            connectionLabel.RemoveFromClassList("status-disconnected");
            connectionLabel.AddToClassList("status-connected");
        }
        else
        {
            connectionLabel.text = "● Disconnected";
            connectionLabel.RemoveFromClassList("status-connected");
            connectionLabel.AddToClassList("status-disconnected");
        }

        if (pingLabel != null)
        {
            pingLabel.text = $"Ping: {PhotonNetwork.GetPing()}ms";
        }
    }

    private void UpdateRoomInfo()
    {
        if (PhotonNetwork.InRoom)
        {
            if (roomCodeText != null) roomCodeText.text = PhotonNetwork.CurrentRoom.Name;
            if (playerCountLabel != null) playerCountLabel.text = $"{PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers} Players";

            // Get table stakes from room properties
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("TableStakes", out object stakes))
            {
                if (stakesDropdown != null) stakesDropdown.index = (int)stakes;
            }

            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("MinBet", out object minBet))
            {
                if (minBetInput != null) minBetInput.value = (int)minBet;
            }

            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("MaxBet", out object maxBet))
            {
                if (maxBetInput != null) maxBetInput.value = (int)maxBet;
            }

            if (startGameButton != null) startGameButton.SetEnabled(PhotonNetwork.IsMasterClient);
        }
        else
        {
            if (roomCodeText != null) roomCodeText.text = "Not in Room";
            if (playerCountLabel != null) playerCountLabel.text = "0/0 Players";
            if (startGameButton != null) startGameButton.SetEnabled(false);
        }

        // Default to European if not in room
        if (tableNameLabel != null) tableNameLabel.text = "European Roulette";
    }

    private void UpdatePlayerList()
    {
        playersInRoom.Clear();

        if (PhotonNetwork.InRoom)
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                playersInRoom.Add(new PlayerInfo
                {
                    Name = player.NickName,
                    IsMaster = player.IsMasterClient,
                    IsLocal = player.IsLocal,
                    ActorNumber = player.ActorNumber
                });
            }
        }

        playersListView?.RefreshItems();
    }

    #endregion

    #region List Binding

    private void BindPlayerItem(VisualElement element, int index)
    {
        if (index < 0 || index >= playersInRoom.Count) return;

        var player = playersInRoom[index];

        var nameLabel = element.Q<Label>("PlayerNameLabel");
        var masterIcon = element.Q<VisualElement>("MasterIcon");
        var kickButton = element.Q<Button>("KickButton");

        if (nameLabel != null) nameLabel.text = player.Name;
        if (masterIcon != null) masterIcon.style.display = player.IsMaster ? DisplayStyle.Flex : DisplayStyle.None;
        if (kickButton != null) kickButton.SetEnabled(PhotonNetwork.IsMasterClient && !player.IsLocal);

        if (kickButton != null && !player.IsLocal)
        {
            kickButton.UnregisterCallback<ClickEvent>(null);
            kickButton.RegisterCallback<ClickEvent>(evt => KickPlayer(player.ActorNumber));
        }
    }

    private void BindTableItem(VisualElement element, int index)
    {
        if (index < 0 || index >= availableTables.Count) return;

        var table = availableTables[index];

        var tableName = element.Q<Label>("TableNameLabel");
        var hostName = element.Q<Label>("HostNameLabel");
        var stakesLabel = element.Q<Label>("StakesLabel");
        var limitsLabel = element.Q<Label>("LimitsLabel");
        var playerCount = element.Q<Label>("PlayerCountLabel");
        var joinButton = element.Q<Button>("JoinTableButton");

        if (tableName != null) tableName.text = table.TableName;
        if (hostName != null) hostName.text = $"Host: {table.HostName}";
        if (stakesLabel != null) stakesLabel.text = table.Stakes.ToString().ToUpper();
        if (limitsLabel != null) limitsLabel.text = $"${table.MinBet} - ${table.MaxBet}";
        if (playerCount != null) playerCount.text = $"{table.PlayerCount}/{table.MaxPlayers}";

        joinButton?.RegisterCallback<ClickEvent>(evt => JoinRoom(table.RoomName));
    }

    #endregion

    #region Actions

    private void CopyRoomCode()
    {
        if (PhotonNetwork.InRoom)
        {
            GUIUtility.systemCopyBuffer = PhotonNetwork.CurrentRoom.Name;
            Debug.Log("Room code copied!");
        }
    }

    private void InvitePlayer()
    {
        // Show invite panel
        Debug.Log("Invite player clicked");
    }

    private void ShareRoomCode()
    {
        // Native share or copy
        CopyRoomCode();
    }

    private void JoinByCode()
    {
        string code = joinCodeInput?.value;
        if (!string.IsNullOrEmpty(code))
        {
            NetworkManager.Instance?.JoinRoom(code);
        }
    }

    private void QuickJoin()
    {
        NetworkManager.Instance?.JoinRandomRoom();
    }

    private void CreatePrivateRoom()
    {
        TableStakes stakes = (TableStakes)(stakesDropdown?.index ?? 1);
        NetworkManager.Instance?.CreateRoom(null, stakes);
    }

    private void LeaveRoom()
    {
        NetworkManager.Instance?.LeaveRoom();
        HidePanel();
    }

    private void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            NetworkGameState.Instance?.StartNewRound();
            HidePanel();
        }
    }

    private void AddBot()
    {
        Debug.Log("Add bot clicked");
    }

    private void JoinRoom(string roomName)
    {
        NetworkManager.Instance?.JoinRoom(roomName);
    }

    private void KickPlayer(int actorNumber)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            if (player != null)
            {
                PhotonNetwork.CloseConnection(player);
            }
        }
    }

    #endregion

    #region Photon Callbacks

    public void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        UpdatePlayerList();
    }

    public void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        UpdatePlayerList();
    }

    #endregion
}

[System.Serializable]
public class PlayerInfo
{
    public string Name;
    public bool IsMaster;
    public bool IsLocal;
    public int ActorNumber;
}

[System.Serializable]
public class TableInfo
{
    public string RoomName;
    public string TableName;
    public string HostName;
    public TableStakes Stakes;
    public int MinBet;
    public int MaxBet;
    public int PlayerCount;
    public int MaxPlayers;
}
