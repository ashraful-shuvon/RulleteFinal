using UnityEngine;
using UnityEngine.UIElements;
using Photon.Pun;
using System.Collections.Generic;

/// <summary>
/// Controller for the in-game HUD (timer, phase, players list)
/// Uses UI Toolkit (UXML/USS)
/// </summary>
public class GameHUDController : MonoBehaviour
{
    [Header("UI Toolkit References")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset playerHUDEntryAsset;

    // Top bar elements
    private Label phaseLabel;
    private VisualElement timerFill;
    private Label timerLabel;
    private Label connectionDot;
    private Label playersLabel;

    // Right panel
    private ListView playersList;
    private Button inviteButton;

    // Bottom bar
    private Label balanceText;
    private Label totalBetText;
    private Label roomCodeLabel;

    // Action buttons
    private Button clearButton;
    private Button undoButton;
    private Button rebetButton;
    private Button doubleButton;
    private Button copyRoomCodeBtn;

    // Result popup
    private VisualElement resultPopup;
    private Label resultNumber;
    private VisualElement resultColor;
    private Label resultWinLabel;
    private Label winAmountLabel;

    // Invite panel
    private VisualElement invitePanel;
    private Label inviteRoomCode;
    private Button copyInviteCodeBtn;
    private Button closeInviteBtn;

    // State
    private List<PlayerHUDInfo> playersHUD = new List<PlayerHUDInfo>();

    private void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();
    }

    private void Start()
    {
        SetupUI();
        SetupButtonCallbacks();
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SetupUI()
    {
        var root = uiDocument.rootVisualElement;

        // Top bar
        phaseLabel = root.Q<Label>("PhaseLabel");
        timerFill = root.Q<VisualElement>("TimerFill");
        timerLabel = root.Q<Label>("TimerLabel");
        connectionDot = root.Q<Label>("ConnectionDot");
        playersLabel = root.Q<Label>("PlayersLabel");

        // Right panel
        playersList = root.Q<ListView>("PlayersList");
        inviteButton = root.Q<Button>("InviteButton");

        // Bottom bar
        balanceText = root.Q<Label>("BalanceText");
        totalBetText = root.Q<Label>("TotalBetText");
        roomCodeLabel = root.Q<Label>("RoomCodeLabel");

        // Action buttons
        clearButton = root.Q<Button>("ClearButton");
        undoButton = root.Q<Button>("UndoButton");
        rebetButton = root.Q<Button>("RebetButton");
        doubleButton = root.Q<Button>("DoubleButton");
        copyRoomCodeBtn = root.Q<Button>("CopyRoomCodeBtn");

        // Result popup
        resultPopup = root.Q<VisualElement>("ResultPopup");
        resultNumber = root.Q<Label>("ResultNumber");
        resultColor = root.Q<VisualElement>("ResultColor");
        resultWinLabel = root.Q<Label>("ResultWinLabel");
        winAmountLabel = root.Q<Label>("WinAmountLabel");

        // Invite panel
        invitePanel = root.Q<VisualElement>("InvitePanel");
        inviteRoomCode = root.Q<Label>("InviteRoomCode");
        copyInviteCodeBtn = root.Q<Button>("CopyInviteCodeBtn");
        closeInviteBtn = root.Q<Button>("CloseInviteBtn");

        // Setup players list
        if (playersList != null && playerHUDEntryAsset != null)
        {
            playersList.makeItem = () => playerHUDEntryAsset.CloneTree();
            playersList.bindItem = BindPlayerHUDItem;
            playersList.itemsSource = playersHUD;
        }

        // Hide popups initially
        HideResultPopup();
        HideInvitePanel();

        // Initial update
        UpdateBalance(3000, 0);
        UpdatePhase(GamePhase.Waiting, 0);
    }

    private void SetupButtonCallbacks()
    {
        clearButton?.RegisterCallback<ClickEvent>(evt => OnClearClicked());
        undoButton?.RegisterCallback<ClickEvent>(evt => OnUndoClicked());
        rebetButton?.RegisterCallback<ClickEvent>(evt => OnRebetClicked());
        doubleButton?.RegisterCallback<ClickEvent>(evt => OnDoubleClicked());
        copyRoomCodeBtn?.RegisterCallback<ClickEvent>(evt => CopyRoomCode());
        inviteButton?.RegisterCallback<ClickEvent>(evt => ShowInvitePanel());
        copyInviteCodeBtn?.RegisterCallback<ClickEvent>(evt => CopyInviteCode());
        closeInviteBtn?.RegisterCallback<ClickEvent>(evt => HideInvitePanel());
    }

    private void SubscribeToEvents()
    {
        if (NetworkGameState.Instance != null)
        {
            NetworkGameState.Instance.OnPhaseChanged += OnPhaseChanged;
            NetworkGameState.Instance.OnTimerUpdated += OnTimerUpdated;
            NetworkGameState.Instance.OnSpinResultReceived += OnSpinResultReceived;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (NetworkGameState.Instance != null)
        {
            NetworkGameState.Instance.OnPhaseChanged -= OnPhaseChanged;
            NetworkGameState.Instance.OnTimerUpdated -= OnTimerUpdated;
            NetworkGameState.Instance.OnSpinResultReceived -= OnSpinResultReceived;
        }
    }

    #region Phase & Timer

    private void OnPhaseChanged(GamePhase phase)
    {
        UpdatePhase(phase, NetworkGameState.Instance.PhaseTimeRemaining);
    }

    private void OnTimerUpdated(float timeRemaining)
    {
        UpdateTimer(timeRemaining);
    }

    public void UpdatePhase(GamePhase phase, float time)
    {
        if (phaseLabel == null) return;

        phaseLabel.text = phase switch
        {
            GamePhase.Waiting => "WAITING",
            GamePhase.Betting => "PLACE YOUR BETS",
            GamePhase.Spinning => "NO MORE BETS",
            GamePhase.Result => "RESULT",
            GamePhase.Payout => "COLLECTING",
            _ => ""
        };

        // Enable/disable action buttons based on phase
        bool canAct = phase == GamePhase.Betting;
        clearButton?.SetEnabled(canAct);
        undoButton?.SetEnabled(canAct);
        rebetButton?.SetEnabled(canAct);
        doubleButton?.SetEnabled(canAct);
    }

    public void UpdateTimer(float timeRemaining)
    {
        if (timerLabel == null) return;

        int seconds = Mathf.CeilToInt(timeRemaining);
        timerLabel.text = seconds.ToString();

        // Update timer bar fill
        if (timerFill != null)
        {
            float maxTime = NetworkGameState.Instance?.CurrentPhase switch
            {
                GamePhase.Betting => 15f,
                GamePhase.Spinning => 7f,
                GamePhase.Result => 3f,
                GamePhase.Payout => 2f,
                _ => 1f
            };
            float fillPercent = Mathf.Clamp01(timeRemaining / maxTime);
            timerFill.style.width = new StyleLength(new Length(fillPercent * 100, LengthUnit.Percent));
        }

        // Flash timer when low
        if (seconds <= 5 && timerLabel != null)
        {
            timerLabel.style.color = Color.red;
        }
        else if (timerLabel != null)
        {
            timerLabel.style.color = Color.white;
        }
    }

    #endregion

    #region Balance

    public void UpdateBalance(float balance, float currentBet)
    {
        if (balanceText != null) balanceText.text = $"${balance:N0}";
        if (totalBetText != null) totalBetText.text = $"Current Bet: ${currentBet:N0}";
    }

    #endregion

    #region Players List

    private void BindPlayerHUDItem(VisualElement element, int index)
    {
        if (index < 0 || index >= playersHUD.Count) return;

        var player = playersHUD[index];

        var nameLabel = element.Q<Label>("NameLabel");
        var balanceLabel = element.Q<Label>("BalanceLabel");
        var betLabel = element.Q<Label>("BetLabel");
        var masterCrown = element.Q<VisualElement>("MasterCrown");
        var readyDot = element.Q<VisualElement>("ReadyDot");

        if (nameLabel != null) nameLabel.text = player.Name;
        if (balanceLabel != null) balanceLabel.text = $"${player.Balance:N0}";
        if (betLabel != null) betLabel.text = player.Bet > 0 ? $"${player.Bet:N0}" : "$0";

        if (masterCrown != null)
        {
            masterCrown.style.display = player.IsMaster ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (readyDot != null)
        {
            readyDot.RemoveFromClassList("not-ready");
            if (!player.IsReady)
                readyDot.AddToClassList("not-ready");
        }
    }

    public void UpdatePlayersList()
    {
        playersHUD.Clear();

        if (PhotonNetwork.InRoom)
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                playersHUD.Add(new PlayerHUDInfo
                {
                    Name = player.NickName,
                    IsMaster = player.IsMasterClient,
                    IsLocal = player.IsLocal,
                    Balance = 3000, // Would come from NetworkPlayer
                    Bet = 0,
                    IsReady = true
                });
            }
        }

        playersList?.RefreshItems();
        if (playersLabel != null) playersLabel.text = $"{PhotonNetwork.CurrentRoom?.PlayerCount ?? 0}/{PhotonNetwork.CurrentRoom?.MaxPlayers ?? 0} Players";
    }

    #endregion

    #region Result Popup

    private void OnSpinResultReceived(int result)
    {
        ShowResult(result, 0, true);
    }

    public void ShowResult(int result, float winAmount, bool isWin)
    {
        if (resultPopup == null) return;

        resultPopup.style.display = DisplayStyle.Flex;
        resultPopup.AddToClassList("animate-scale-in");

        if (resultNumber != null)
        {
            resultNumber.text = result == 37 ? "00" : result.ToString();
        }

        // Set color
        if (resultColor != null)
        {
            resultColor.RemoveFromClassList("red");
            resultColor.RemoveFromClassList("black");
            resultColor.RemoveFromClassList("green");

            if (result == 0 || result == 37)
                resultColor.AddToClassList("green");
            else if (IsRedNumber(result))
                resultColor.AddToClassList("red");
            else
                resultColor.AddToClassList("black");
        }

        // Win/lose text
        if (resultWinLabel != null)
        {
            resultWinLabel.text = isWin ? "WIN!" : "LOSE";
            resultWinLabel.style.color = isWin ? new Color(212f/255, 175f/255, 55f/255) : Color.red;
        }

        if (winAmountLabel != null)
        {
            winAmountLabel.text = isWin ? $"+${winAmount:N0}" : "";
            winAmountLabel.style.display = isWin ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // Auto-hide after delay
        Invoke(nameof(HideResultPopup), 4f);
    }

    public void HideResultPopup()
    {
        if (resultPopup == null) return;
        resultPopup.style.display = DisplayStyle.None;
        resultPopup.RemoveFromClassList("animate-scale-in");
    }

    private bool IsRedNumber(int number)
    {
        int[] redNumbers = { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
        foreach (int red in redNumbers)
            if (red == number) return true;
        return false;
    }

    #endregion

    #region Invite Panel

    public void ShowInvitePanel()
    {
        if (invitePanel == null) return;

        invitePanel.style.display = DisplayStyle.Flex;
        invitePanel.AddToClassList("animate-scale-in");

        if (inviteRoomCode != null && PhotonNetwork.InRoom)
        {
            inviteRoomCode.text = PhotonNetwork.CurrentRoom.Name;
        }
    }

    public void HideInvitePanel()
    {
        if (invitePanel == null) return;
        invitePanel.style.display = DisplayStyle.None;
    }

    #endregion

    #region Button Actions

    private void OnClearClicked()
    {
        NetworkPlayer.LocalPlayer?.ClearAllBets();
    }

    private void OnUndoClicked()
    {
        NetworkPlayer.LocalPlayer?.UndoLastBet();
    }

    private void OnRebetClicked()
    {
        NetworkPlayer.LocalPlayer?.RepeatLastBets();
    }

    private void OnDoubleClicked()
    {
        NetworkPlayer.LocalPlayer?.DoubleBets();
    }

    private void CopyRoomCode()
    {
        if (PhotonNetwork.InRoom)
        {
            GUIUtility.systemCopyBuffer = PhotonNetwork.CurrentRoom.Name;
        }
    }

    private void CopyInviteCode()
    {
        if (PhotonNetwork.InRoom)
        {
            GUIUtility.systemCopyBuffer = PhotonNetwork.CurrentRoom.Name;
        }
    }

    #endregion
}

[System.Serializable]
public class PlayerHUDInfo
{
    public string Name;
    public bool IsMaster;
    public bool IsLocal;
    public float Balance;
    public float Bet;
    public bool IsReady;
}
