using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;

/// <summary>
/// Handles synchronized game state across all clients - betting phases, spin results, and payouts
/// </summary>
public class NetworkGameState : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static NetworkGameState Instance;

    [Header("Timing Settings")]
    [SerializeField] private float bettingPhaseDuration = 15f;
    [SerializeField] private float spinningPhaseDuration = 7f;
    [SerializeField] private float resultPhaseDuration = 3f;
    [SerializeField] private float payoutPhaseDuration = 2f;

    [Header("References")]
    [SerializeField] private EuropeanWheel europeanWheel;
    [SerializeField] private AmericanWheel americanWheel;

    // Current game state
    public GamePhase CurrentPhase { get; private set; } = GamePhase.Waiting;
    public float PhaseTimeRemaining { get; private set; } = 0f;
    public int LastResult { get; private set; } = -1;
    public bool IsEuropeanWheel { get; private set; } = true;

    // Events
    public System.Action<GamePhase> OnPhaseChanged;
    public System.Action<float> OnTimerUpdated;
    public System.Action<int> OnSpinResultReceived;
    public System.Action<float> OnPayoutReceived;

    // Player bets storage (synced)
    private Dictionary<int, PlayerBetData> playerBets = new Dictionary<int, PlayerBetData>();

    // Photon Event Codes
    private const byte PHASE_CHANGE_EVENT = 1;
    private const byte SPIN_RESULT_EVENT = 2;
    private const byte PLAYER_BET_EVENT = 3;
    private const byte CLEAR_BETS_EVENT = 4;
    private const byte TIMER_SYNC_EVENT = 5;

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
        // Find wheel references if not set
        if (europeanWheel == null)
            europeanWheel = FindObjectOfType<EuropeanWheel>();
        if (americanWheel == null)
            americanWheel = FindObjectOfType<AmericanWheel>();

        // Determine wheel type from scene
        IsEuropeanWheel = europeanWheel != null && europeanWheel.isActiveAndEnabled;
        
        // Initialize bet space registry
        BetSpaceRegistry.Initialize();

        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && CurrentPhase == GamePhase.Waiting)
        {
            StartNewRound();
        }
    }

    private void Update()
    {
        // Only Master Client manages the timer
        if (!PhotonNetwork.IsMasterClient) return;

        if (CurrentPhase != GamePhase.Waiting)
        {
            PhaseTimeRemaining -= Time.deltaTime;
            
            // Broadcast timer sync every second
            if (Time.frameCount % 60 == 0)
            {
                SendTimerSync(PhaseTimeRemaining);
            }

            OnTimerUpdated?.Invoke(PhaseTimeRemaining);

            // Phase transitions
            if (PhaseTimeRemaining <= 0)
            {
                AdvancePhase();
            }
        }
    }

    #region Phase Management

    /// <summary>
    /// Start a new betting round (Master Client only)
    /// </summary>
    public void StartNewRound()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log("[NetworkGameState] Starting new round");
        SetPhase(GamePhase.Betting, bettingPhaseDuration);
    }

    /// <summary>
    /// Advance to the next phase (Master Client only)
    /// </summary>
    private void AdvancePhase()
    {
        GamePhase nextPhase = CurrentPhase switch
        {
            GamePhase.Betting => GamePhase.Spinning,
            GamePhase.Spinning => GamePhase.Result,
            GamePhase.Result => GamePhase.Payout,
            GamePhase.Payout => GamePhase.Betting,
            GamePhase.Waiting => GamePhase.Betting,
            _ => GamePhase.Betting
        };

        float duration = nextPhase switch
        {
            GamePhase.Betting => bettingPhaseDuration,
            GamePhase.Spinning => spinningPhaseDuration,
            GamePhase.Result => resultPhaseDuration,
            GamePhase.Payout => payoutPhaseDuration,
            _ => 0f
        };

        switch (nextPhase)
        {
            case GamePhase.Spinning:
                GenerateAndBroadcastResult();
                break;
            case GamePhase.Payout:
                CalculatePayouts();
                break;
            case GamePhase.Betting:
                ClearAllBets();
                if (SceneRoulette._Instance != null && SceneRoulette._Instance.camCtrl != null)
                {
                    SceneRoulette._Instance.camCtrl.GoToOrigin();
                }
                break;
        }

        SetPhase(nextPhase, duration);
    }

    /// <summary>
    /// Set game phase and broadcast to all clients
    /// </summary>
    private void SetPhase(GamePhase phase, float duration)
    {
        CurrentPhase = phase;
        PhaseTimeRemaining = duration;

        // Update room properties
        if (PhotonNetwork.InRoom)
        {
            Hashtable props = new Hashtable
            {
                { "GamePhase", (int)phase },
                { "PhaseEndTime", PhotonNetwork.Time + duration }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        // Broadcast phase change event
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(PHASE_CHANGE_EVENT, new object[] { (int)phase, duration }, options, SendOptions.SendReliable);

        Debug.Log($"[NetworkGameState] Phase changed to: {phase}");
        OnPhaseChanged?.Invoke(phase);
    }

    #endregion

    #region Spin Result

    /// <summary>
    /// Generate result on Master Client and broadcast
    /// </summary>
    private void GenerateAndBroadcastResult()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // Generate random result based on wheel type
        int result = IsEuropeanWheel ? Random.Range(0, 37) : Random.Range(0, 38);
        LastResult = result;

        Debug.Log($"[NetworkGameState] Generated result: {result}");

        // Broadcast to all clients
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(SPIN_RESULT_EVENT, result, options, SendOptions.SendReliable);

        // Trigger wheel spin on all clients
        TriggerWheelSpin(result);
    }

    /// <summary>
    /// Called when spin result is received from Master
    /// </summary>
    private void OnSpinResultReceivedFromMaster(int result)
    {
        LastResult = result;
        Debug.Log($"[NetworkGameState] Received spin result: {result}");
        OnSpinResultReceived?.Invoke(result);

        // Trigger wheel animation
        TriggerWheelSpin(result);
    }

    private void TriggerWheelSpin(int result)
    {
        Debug.Log($"[NetworkGameState] Triggering wheel spin for result: {result}");
        
        if (SceneRoulette._Instance != null && SceneRoulette._Instance.camCtrl != null)
        {
            SceneRoulette._Instance.camCtrl.GoToTarget();
        }

        if (IsEuropeanWheel && europeanWheel != null)
        {
            europeanWheel.SpinToResult(result);
        }
        else if (!IsEuropeanWheel && americanWheel != null)
        {
            americanWheel.SpinToResult(result);
        }
    }

    #endregion

    #region Betting System

    /// <summary>
    /// Player places a bet - broadcast to all clients
    /// </summary>
    public void PlaceBet(int betSpaceIndex, float amount)
    {
        if (CurrentPhase != GamePhase.Betting)
        {
            Debug.LogWarning("[NetworkGameState] Cannot place bet - not in betting phase");
            return;
        }

        int playerId = PhotonNetwork.LocalPlayer.ActorNumber;

        // Send bet event
        object[] betData = new object[] { playerId, betSpaceIndex, amount };
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(PLAYER_BET_EVENT, betData, options, SendOptions.SendReliable);

        Debug.Log($"[NetworkGameState] Player {playerId} bet {amount} on space {betSpaceIndex}");
    }

    /// <summary>
    /// Process received bet from another player
    /// </summary>
    private void OnPlayerBetReceived(int playerId, int betSpaceIndex, float amount)
    {
        Debug.Log($"[NetworkGameState] Received bet from player {playerId}: {amount} on space {betSpaceIndex}");

        if (!playerBets.ContainsKey(playerId))
        {
            playerBets[playerId] = new PlayerBetData { PlayerId = playerId };
        }

        playerBets[playerId].AddBet(betSpaceIndex, amount);

        // Notify UI to show other player's bets
        // This would be connected to the visual chip system
    }

    /// <summary>
    /// Clear all bets for new round
    /// </summary>
    private void ClearAllBets()
    {
        playerBets.Clear();
        
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(CLEAR_BETS_EVENT, null, options, SendOptions.SendReliable);

        Debug.Log("[NetworkGameState] All bets cleared");
    }

    /// <summary>
    /// Get total bets for a specific player
    /// </summary>
    public float GetPlayerTotalBet(int playerId)
    {
        if (playerBets.TryGetValue(playerId, out PlayerBetData betData))
        {
            return betData.TotalBet;
        }
        return 0f;
    }

    /// <summary>
    /// Get all player bets
    /// </summary>
    public Dictionary<int, PlayerBetData> GetAllPlayerBets()
    {
        return playerBets;
    }

    #endregion

    #region Payout System

    /// <summary>
    /// Calculate payouts for all players (Master Client only)
    /// </summary>
    private void CalculatePayouts()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log("[NetworkGameState] Calculating payouts...");

        foreach (var kvp in playerBets)
        {
            int playerId = kvp.Key;
            PlayerBetData betData = kvp.Value;

            float totalWin = betData.CalculateWinnings(LastResult);

            if (totalWin > 0)
            {
                Debug.Log($"[NetworkGameState] Player {playerId} wins: {totalWin}");
                OnPayoutReceived?.Invoke(totalWin);
                
                // In a full implementation, this would update the player's balance
                // via NetworkPlayerBalance component
            }
        }
    }

    #endregion

    #region Photon Events

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case PHASE_CHANGE_EVENT:
                HandlePhaseChangeEvent(photonEvent);
                break;
            case SPIN_RESULT_EVENT:
                HandleSpinResultEvent(photonEvent);
                break;
            case PLAYER_BET_EVENT:
                HandlePlayerBetEvent(photonEvent);
                break;
            case CLEAR_BETS_EVENT:
                HandleClearBetsEvent();
                break;
            case TIMER_SYNC_EVENT:
                HandleTimerSyncEvent(photonEvent);
                break;
        }
    }

    private void HandlePhaseChangeEvent(EventData photonEvent)
    {
        object[] data = (object[])photonEvent.CustomData;
        GamePhase phase = (GamePhase)data[0];
        float duration = (float)data[1];

        CurrentPhase = phase;
        PhaseTimeRemaining = duration;

        Debug.Log($"[NetworkGameState] Phase change received: {phase}");
        OnPhaseChanged?.Invoke(phase);
    }

    private void HandleSpinResultEvent(EventData photonEvent)
    {
        int result = (int)photonEvent.CustomData;
        OnSpinResultReceivedFromMaster(result);
    }

    private void HandlePlayerBetEvent(EventData photonEvent)
    {
        object[] data = (object[])photonEvent.CustomData;
        int playerId = (int)data[0];
        int betSpaceIndex = (int)data[1];
        float amount = (float)data[2];

        OnPlayerBetReceived(playerId, betSpaceIndex, amount);
    }

    private void HandleClearBetsEvent()
    {
        playerBets.Clear();
        Debug.Log("[NetworkGameState] Bets cleared event received");
    }

    private void HandleTimerSyncEvent(EventData photonEvent)
    {
        float timeRemaining = (float)photonEvent.CustomData;
        PhaseTimeRemaining = timeRemaining;
        OnTimerUpdated?.Invoke(timeRemaining);
    }

    private void SendTimerSync(float timeRemaining)
    {
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(TIMER_SYNC_EVENT, timeRemaining, options, SendOptions.SendReliable);
    }

    #endregion

    #region Room Property Callbacks

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        if (PhotonNetwork.IsMasterClient && CurrentPhase == GamePhase.Waiting)
        {
            StartNewRound();
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.TryGetValue("GamePhase", out object phase))
        {
            GamePhase newPhase = (GamePhase)(int)phase;
            if (newPhase != CurrentPhase)
            {
                CurrentPhase = newPhase;
                OnPhaseChanged?.Invoke(newPhase);
            }
        }

        if (propertiesThatChanged.TryGetValue("PhaseEndTime", out object endTime))
        {
            double endTimeDouble = (double)endTime;
            PhaseTimeRemaining = (float)(endTimeDouble - PhotonNetwork.Time);
        }
    }

    #endregion

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}

/// <summary>
/// Data class to store a player's bets for a round
/// </summary>
[System.Serializable]
public class PlayerBetData
{
    public int PlayerId;
    public Dictionary<int, float> Bets = new Dictionary<int, float>(); // betSpaceIndex -> amount
    public float TotalBet;

    public void AddBet(int betSpaceIndex, float amount)
    {
        if (Bets.ContainsKey(betSpaceIndex))
        {
            Bets[betSpaceIndex] += amount;
        }
        else
        {
            Bets[betSpaceIndex] = amount;
        }
        TotalBet += amount;
    }

    public float CalculateWinnings(int result)
    {
        float totalWinnings = 0f;

        foreach (var kvp in Bets)
        {
            int betSpaceIndex = kvp.Key;
            float betAmount = kvp.Value;

            // Get the BetSpace using BetSpaceRegistry
            BetSpace betSpace = BetSpaceRegistry.GetBetSpaceByIndex(betSpaceIndex);
            if (betSpace != null)
            {
                // Check if this bet wins
                foreach (int winningNum in betSpace.winningNumbers)
                {
                    if (winningNum == result)
                    {
                        // Calculate winnings based on bet type
                        int multiplier = GetPayoutMultiplier(betSpace.betType);
                        totalWinnings += betAmount * multiplier + betAmount; // Include original bet
                        break;
                    }
                }
            }
        }

        return totalWinnings;
    }

    private int GetPayoutMultiplier(BetType betType)
    {
        return betType switch
        {
            BetType.Straight => 35,
            BetType.Split => 17,
            BetType.Street => 11,
            BetType.Corner => 8,
            BetType.DoubleStreet => 5,
            BetType.Row => 2,
            BetType.Dozen => 2,
            BetType.Red => 1,
            BetType.Black => 1,
            BetType.Even => 1,
            BetType.Odd => 1,
            BetType.Low => 1,
            BetType.High => 1,
            _ => 0
        };
    }
}
