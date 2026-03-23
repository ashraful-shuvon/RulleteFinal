using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// In-game UI manager for multiplayer roulette - handles timer, phase display, and player list
/// </summary>
public class GameUIManager : MonoBehaviourPunCallbacks
{
    public static GameUIManager Instance;

    [Header("Phase & Timer")]
    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private Image phaseBackground;

    [Header("Result Display")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultNumberText;
    [SerializeField] private Image resultColorIndicator;

    [Header("Player List")]
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerEntryPrefab;

    [Header("Betting UI")]
    [SerializeField] private Button spinButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button undoButton;
    [SerializeField] private Button rebetButton;
    [SerializeField] private Button doubleButton;
    [SerializeField] private Button leaveButton;

    [Header("Balance Display")]
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text totalBetText;
    [SerializeField] private TMP_Text lastWinText;

    [Header("Betting Phase")]
    [SerializeField] private Color bettingColor = new Color(0.2f, 0.8f, 0.2f); // Green
    [SerializeField] private Color spinningColor = new Color(0.8f, 0.6f, 0.2f); // Orange
    [SerializeField] private Color resultColor = new Color(0.8f, 0.2f, 0.2f); // Red

    [Header("Win Animation")]
    [SerializeField] private GameObject winPopup;
    [SerializeField] private TMP_Text winAmountText;
    [SerializeField] private Animator winAnimator;

    private List<PlayerEntryUI> playerEntries = new List<PlayerEntryUI>();

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
        SetupButtons();
        UpdatePhaseUI(GamePhase.Waiting, 0f);
        UpdateBalanceDisplay();

        // Hide result panel initially
        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (winPopup != null)
            winPopup.SetActive(false);

        // Subscribe to events
        if (NetworkGameState.Instance != null)
        {
            NetworkGameState.Instance.OnPhaseChanged += OnPhaseChanged;
            NetworkGameState.Instance.OnTimerUpdated += OnTimerUpdated;
            NetworkGameState.Instance.OnSpinResultReceived += OnSpinResult;
        }

        // Create player entries for existing players
        RefreshPlayerList();
    }

    private void OnDestroy()
    {
        if (NetworkGameState.Instance != null)
        {
            NetworkGameState.Instance.OnPhaseChanged -= OnPhaseChanged;
            NetworkGameState.Instance.OnTimerUpdated -= OnTimerUpdated;
            NetworkGameState.Instance.OnSpinResultReceived -= OnSpinResult;
        }
    }

    private void SetupButtons()
    {
        if (spinButton != null)
            spinButton.onClick.AddListener(OnSpinClicked);

        if (clearButton != null)
            clearButton.onClick.AddListener(OnClearClicked);

        if (undoButton != null)
            undoButton.onClick.AddListener(OnUndoClicked);

        if (rebetButton != null)
            rebetButton.onClick.AddListener(OnRebetClicked);

        if (doubleButton != null)
            doubleButton.onClick.AddListener(OnDoubleClicked);

        if (leaveButton != null)
            leaveButton.onClick.AddListener(OnLeaveClicked);
    }

    #region Phase & Timer

    private void OnPhaseChanged(GamePhase phase)
    {
        UpdatePhaseUI(phase, NetworkGameState.Instance.PhaseTimeRemaining);
    }

    private void OnTimerUpdated(float timeRemaining)
    {
        UpdateTimerDisplay(timeRemaining);
    }

    private void UpdatePhaseUI(GamePhase phase, float time)
    {
        // Update phase text
        if (phaseText != null)
        {
            phaseText.text = phase switch
            {
                GamePhase.Waiting => "WAITING FOR PLAYERS",
                GamePhase.Betting => "PLACE YOUR BETS",
                GamePhase.Spinning => "NO MORE BETS",
                GamePhase.Result => "RESULT",
                GamePhase.Payout => "COLLECTING",
                _ => ""
            };
        }

        // Update background color
        if (phaseBackground != null)
        {
            phaseBackground.color = phase switch
            {
                GamePhase.Betting => bettingColor,
                GamePhase.Spinning => spinningColor,
                GamePhase.Result => resultColor,
                GamePhase.Payout => resultColor,
                _ => Color.gray
            };
        }

        // Enable/disable betting buttons
        bool canBet = phase == GamePhase.Betting;

        if (spinButton != null)
            spinButton.interactable = canBet && PhotonNetwork.IsMasterClient;

        if (clearButton != null)
            clearButton.interactable = canBet;

        if (undoButton != null)
            undoButton.interactable = canBet;

        if (rebetButton != null)
            rebetButton.interactable = canBet;

        if (doubleButton != null)
            doubleButton.interactable = canBet;
    }

    private void UpdateTimerDisplay(float timeRemaining)
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(timeRemaining);
            timerText.text = seconds.ToString();

            // Flash when time is running out
            if (seconds <= 5)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }
        }

        if (timerSlider != null)
        {
            float maxValue = NetworkGameState.Instance.CurrentPhase switch
            {
                GamePhase.Betting => 15f,
                GamePhase.Spinning => 7f,
                GamePhase.Result => 3f,
                GamePhase.Payout => 2f,
                _ => 1f
            };

            timerSlider.maxValue = maxValue;
            timerSlider.value = timeRemaining;
        }
    }

    #endregion

    #region Result Display

    private void OnSpinResult(int result)
    {
        ShowResult(result);
    }

    private void ShowResult(int result)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (resultNumberText != null)
        {
            if (result == 37)
            {
                resultNumberText.text = "00";
            }
            else
            {
                resultNumberText.text = result.ToString();
            }
        }

        if (resultColorIndicator != null)
        {
            resultColorIndicator.color = GetNumberColor(result);
        }

        // Hide after delay
        StartCoroutine(HideResultAfterDelay(5f));
    }

    private Color GetNumberColor(int number)
    {
        if (number == 0 || number == 37) // 37 is 00
            return Color.green;

        int[] redNumbers = { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };

        foreach (int red in redNumbers)
        {
            if (red == number)
                return Color.red;
        }

        return Color.black;
    }

    private System.Collections.IEnumerator HideResultAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    #endregion

    #region Balance Display

    public void UpdateBalanceDisplay()
    {
        if (NetworkPlayer.LocalPlayer != null)
        {
            if (balanceText != null)
            {
                balanceText.text = $"${NetworkPlayer.LocalPlayer.Balance:N0}";
            }

            if (totalBetText != null)
            {
                totalBetText.text = $"Bet: ${NetworkPlayer.LocalPlayer.CurrentBet:N0}";
            }
        }
    }

    public void ShowWinPopup(float amount)
    {
        if (winPopup != null && winAmountText != null)
        {
            winPopup.SetActive(true);
            winAmountText.text = $"+${amount:N0}";

            if (winAnimator != null)
            {
                winAnimator.SetTrigger("ShowWin");
            }

            StartCoroutine(HideWinPopupAfterDelay(3f));
        }

        if (lastWinText != null)
        {
            lastWinText.text = $"Last Win: ${amount:N0}";
        }
    }

    private System.Collections.IEnumerator HideWinPopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (winPopup != null)
            winPopup.SetActive(false);
    }

    #endregion

    #region Player List

    private void RefreshPlayerList()
    {
        // Clear existing entries
        foreach (var entry in playerEntries)
        {
            if (entry != null)
                Destroy(entry.gameObject);
        }
        playerEntries.Clear();

        // Create entries for all players
        foreach (var player in PhotonNetwork.PlayerList)
        {
            CreatePlayerEntry(player);
        }
    }

    private void CreatePlayerEntry(Player player)
    {
        if (playerEntryPrefab == null || playerListContainer == null) return;

        GameObject entry = Instantiate(playerEntryPrefab, playerListContainer);

        PlayerEntryUI entryUI = entry.GetComponent<PlayerEntryUI>();
        if (entryUI != null)
        {
            entryUI.Setup(player);
            playerEntries.Add(entryUI);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        CreatePlayerEntry(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Remove entry
        PlayerEntryUI entry = playerEntries.Find(e => e.Player == otherPlayer);
        if (entry != null)
        {
            playerEntries.Remove(entry);
            Destroy(entry.gameObject);
        }
    }

    #endregion

    #region Button Handlers

    private void OnSpinClicked()
    {
        // For testing - manually start spin (only master should do this)
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[GameUIManager] Spin button clicked - starting round");
            NetworkGameState.Instance.StartNewRound();
        }
    }

    private void OnClearClicked()
    {
        Debug.Log("[GameUIManager] Clear button clicked");

        if (NetworkPlayer.LocalPlayer != null)
        {
            NetworkPlayer.LocalPlayer.ClearAllBets();
            UpdateBalanceDisplay();
        }
    }

    private void OnUndoClicked()
    {
        Debug.Log("[GameUIManager] Undo button clicked");

        if (NetworkPlayer.LocalPlayer != null)
        {
            NetworkPlayer.LocalPlayer.UndoLastBet();
            UpdateBalanceDisplay();
        }
    }

    private void OnRebetClicked()
    {
        Debug.Log("[GameUIManager] Rebet button clicked");

        if (NetworkPlayer.LocalPlayer != null)
        {
            NetworkPlayer.LocalPlayer.RepeatLastBets();
            UpdateBalanceDisplay();
        }
    }

    private void OnDoubleClicked()
    {
        Debug.Log("[GameUIManager] Double button clicked");

        if (NetworkPlayer.LocalPlayer != null)
        {
            NetworkPlayer.LocalPlayer.DoubleBets();
            UpdateBalanceDisplay();
        }
    }

    private void OnLeaveClicked()
    {
        Debug.Log("[GameUIManager] Leave button clicked");
        NetworkManager.Instance.LeaveRoom();
    }

    #endregion

    #region Utility

    public void SetInteractable(bool interactable)
    {
        if (clearButton != null)
            clearButton.interactable = interactable;

        if (undoButton != null)
            undoButton.interactable = interactable;

        if (rebetButton != null)
            rebetButton.interactable = interactable;

        if (doubleButton != null)
            doubleButton.interactable = interactable;
    }

    #endregion
}
