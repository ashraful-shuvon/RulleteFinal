using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

/// <summary>
/// Represents a networked player in the roulette game - handles betting and balance
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class NetworkPlayer : MonoBehaviourPun
{
    public static NetworkPlayer LocalPlayer;

    [Header("Player Info")]
    [SerializeField] private int seatIndex = -1;
    [SerializeField] private Color playerColor = Color.white;

    [Header("References")]
    [SerializeField] private SpriteRenderer avatarRenderer;
    [SerializeField] private TMPro.TMP_Text nameLabel;

    // Player state
    public float Balance { get; private set; }
    public float CurrentBet { get; private set; }
    public bool IsReady { get; private set; }
    public int SeatIndex => seatIndex;

    // Player bets for current round
    private Dictionary<int, float> currentRoundBets = new Dictionary<int, float>();
    private Dictionary<int, float> previousRoundBets = new Dictionary<int, float>();
    private List<BetRecord> betHistory = new List<BetRecord>();

    // Events
    public System.Action<float> OnBalanceChanged;
    public System.Action<float> OnBetPlaced;
    public System.Action<float> OnWinReceived;
    public System.Action<int> OnSeatAssigned;

    // Constants
    private const float STARTING_BALANCE = 3000f;

    private void Awake()
    {
        if (photonView.IsMine)
        {
            LocalPlayer = this;
        }
    }

    private void Start()
    {
        // Initialize balance
        if (photonView.IsMine)
        {
            Balance = STARTING_BALANCE;
            OnBalanceChanged?.Invoke(Balance);
        }

        // Set name
        if (nameLabel != null)
        {
            nameLabel.text = photonView.Owner.NickName;
        }

        if (photonView.IsMine && NetworkGameState.Instance != null)
        {
            NetworkGameState.Instance.OnPhaseChanged += HandlePhaseChanged;
        }
    }

    private void OnDestroy()
    {
        if (photonView.IsMine && NetworkGameState.Instance != null)
        {
            NetworkGameState.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }
    }

    #region Betting Methods

    /// <summary>
    /// Place a bet on a betting space
    /// </summary>
    public bool PlaceBetOnSpace(int betSpaceIndex, float amount)
    {
        if (!CanPlaceBet(amount))
        {
            Debug.LogWarning($"[NetworkPlayer] Cannot place bet - insufficient balance");
            return false;
        }

        if (NetworkGameState.Instance == null || NetworkGameState.Instance.CurrentPhase != GamePhase.Betting)
        {
            Debug.LogWarning($"[NetworkPlayer] Cannot place bet - NetworkGameState is missing or not in betting phase");
            return false;
        }

        // Deduct from balance
        Balance -= amount;
        CurrentBet += amount;

        // Record the bet
        if (currentRoundBets.ContainsKey(betSpaceIndex))
        {
            currentRoundBets[betSpaceIndex] += amount;
        }
        else
        {
            currentRoundBets[betSpaceIndex] = amount;
        }

        // Store for undo
        betHistory.Add(new BetRecord { BetSpaceIndex = betSpaceIndex, Amount = amount });

        // Notify network
        if (NetworkGameState.Instance != null)
            NetworkGameState.Instance.PlaceBet(betSpaceIndex, amount);

        // Trigger events
        OnBalanceChanged?.Invoke(Balance);
        OnBetPlaced?.Invoke(amount);

        Debug.Log($"[NetworkPlayer] Placed bet of {amount} on space {betSpaceIndex}. Remaining balance: {Balance}");

        return true;
    }

    /// <summary>
    /// Check if player can place a bet
    /// </summary>
    public bool CanPlaceBet(float amount)
    {
        return Balance >= amount;
    }

    /// <summary>
    /// Undo last bet
    /// </summary>
    public bool UndoLastBet()
    {
        if (betHistory.Count == 0) return false;

        BetRecord lastBet = betHistory[betHistory.Count - 1];
        betHistory.RemoveAt(betHistory.Count - 1);

        // Restore balance
        Balance += lastBet.Amount;
        CurrentBet -= lastBet.Amount;

        // Update bet records
        if (currentRoundBets.ContainsKey(lastBet.BetSpaceIndex))
        {
            currentRoundBets[lastBet.BetSpaceIndex] -= lastBet.Amount;
            if (currentRoundBets[lastBet.BetSpaceIndex] <= 0)
            {
                currentRoundBets.Remove(lastBet.BetSpaceIndex);
            }
        }

        if (NetworkGameState.Instance != null)
        {
            NetworkGameState.Instance.PlayerUndoBet(lastBet.BetSpaceIndex, lastBet.Amount);
        }

        OnBalanceChanged?.Invoke(Balance);

        // Update visuals
        BetSpace space = BetSpaceRegistry.GetBetSpaceByIndex(lastBet.BetSpaceIndex);
        if (space != null && space.stack != null)
        {
            space.stack.Remove(lastBet.Amount);
        }

        Debug.Log($"[NetworkPlayer] Undid bet of {lastBet.Amount} on space {lastBet.BetSpaceIndex}");

        return true;
    }

    /// <summary>
    /// Clear all bets for current round
    /// </summary>
    public void ClearAllBets()
    {
        // Restore balance
        Balance += CurrentBet;
        CurrentBet = 0;

        // Clear visuals
        foreach (var bet in betHistory)
        {
            BetSpace space = BetSpaceRegistry.GetBetSpaceByIndex(bet.BetSpaceIndex);
            if (space != null && space.stack != null)
            {
                space.stack.Clear();
            }
        }

        // Clear records
        previousRoundBets.Clear();
        currentRoundBets.Clear();
        betHistory.Clear();

        if (NetworkGameState.Instance != null)
        {
            NetworkGameState.Instance.PlayerClearBets();
        }

        OnBalanceChanged?.Invoke(Balance);

        Debug.Log($"[NetworkPlayer] Cleared all bets. Balance restored: {Balance}");
    }

    /// <summary>
    /// Repeat last round's bets
    /// </summary>
    public void RepeatLastBets()
    {
        if (previousRoundBets == null || previousRoundBets.Count == 0)
        {
            Debug.LogWarning("[NetworkPlayer] No previous bets to repeat.");
            return;
        }

        float totalNeeded = 0;
        foreach (var amount in previousRoundBets.Values) totalNeeded += amount;

        if (Balance < totalNeeded)
        {
            Debug.LogWarning("[NetworkPlayer] Insufficient balance to rebet.");
            return;
        }

        foreach (var kvp in previousRoundBets)
        {
            if (PlaceBetOnSpace(kvp.Key, kvp.Value))
            {
                BetSpace space = BetSpaceRegistry.GetBetSpaceByIndex(kvp.Key);
                if (space != null && space.stack != null)
                {
                    space.stack.Add(kvp.Value);
                }
            }
        }

        Debug.Log($"[NetworkPlayer] Repeating last bets. Total placed: {totalNeeded}");
    }

    /// <summary>
    /// Double all current bets
    /// </summary>
    public bool DoubleBets()
    {
        float totalToDouble = CurrentBet;
        
        if (Balance < totalToDouble)
        {
            Debug.LogWarning($"[NetworkPlayer] Cannot double bets - insufficient balance");
            return false;
        }

        // Double each bet
        Dictionary<int, float> betsCopy = new Dictionary<int, float>(currentRoundBets);
        foreach (var kvp in betsCopy)
        {
            if (PlaceBetOnSpace(kvp.Key, kvp.Value))
            {
                BetSpace space = BetSpaceRegistry.GetBetSpaceByIndex(kvp.Key);
                if (space != null && space.stack != null)
                {
                    space.stack.Add(kvp.Value);
                }
            }
        }

        Debug.Log($"[NetworkPlayer] Doubled all bets. Total bet: {CurrentBet}");
        return true;
    }

    #endregion

    #region Winnings

    /// <summary>
    /// Receive winnings from a round
    /// </summary>
    public void ReceiveWinnings(float amount)
    {
        Balance += amount;
        
        // Save round bets for Rebet before clearing
        previousRoundBets = new Dictionary<int, float>(currentRoundBets);
        
        CurrentBet = 0;

        // Clear for next round
        currentRoundBets.Clear();
        betHistory.Clear();

        OnBalanceChanged?.Invoke(Balance);
        OnWinReceived?.Invoke(amount);

        Debug.Log($"[NetworkPlayer] Received winnings: {amount}. New balance: {Balance}");
    }

    public float CalculatePotentialWin(int result)
    {
        float totalWin = 0;
        foreach(var kvp in currentRoundBets)
        {
            BetSpace space = BetSpaceRegistry.GetBetSpaceByIndex(kvp.Key);
            if (space != null)
            {
                foreach (int winNum in space.winningNumbers)
                {
                    if (winNum == result)
                    {
                        int multiplier = GetPayoutMultiplier(space.betType);
                        totalWin += kvp.Value * multiplier + kvp.Value;
                        break;
                    }
                }
            }
        }
        return totalWin;
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

    private void HandlePhaseChanged(GamePhase phase)
    {
        if (photonView.IsMine && phase == GamePhase.Payout)
        {
            float totalWin = CalculatePotentialWin(NetworkGameState.Instance.LastResult);
            ReceiveWinnings(totalWin);
        }
    }

    #endregion

    #region Seat Management

    /// <summary>
    /// Assign player to a seat
    /// </summary>
    public void AssignSeat(int seat)
    {
        seatIndex = seat;
        OnSeatAssigned?.Invoke(seat);
        Debug.Log($"[NetworkPlayer] Assigned to seat {seat}");
    }

    #endregion

    #region Ready State

    /// <summary>
    /// Set player ready state
    /// </summary>
    public void SetReady(bool ready)
    {
        IsReady = ready;
        // Could sync via RPC if needed
        photonView.RPC("RpcSetReady", RpcTarget.Others, ready);
    }

    [PunRPC]
    private void RpcSetReady(bool ready)
    {
        IsReady = ready;
    }

    #endregion

    #region Serialization (for Photon)

    /// <summary>
    /// Sync balance and bet data across network
    /// </summary>
    public void SyncPlayerState()
    {
        photonView.RPC("RpcSyncState", RpcTarget.Others, Balance, CurrentBet, seatIndex);
    }

    [PunRPC]
    private void RpcSyncState(float balance, float currentBet, int seat)
    {
        Balance = balance;
        CurrentBet = currentBet;
        seatIndex = seat;
        OnBalanceChanged?.Invoke(Balance);
    }

    #endregion

    #region Static Helpers

    /// <summary>
    /// Get all network players in scene
    /// </summary>
    public static NetworkPlayer[] GetAllPlayers()
    {
        return FindObjectsOfType<NetworkPlayer>();
    }

    /// <summary>
    /// Get player by actor number
    /// </summary>
    public static NetworkPlayer GetPlayerByActorNumber(int actorNumber)
    {
        foreach (var player in GetAllPlayers())
        {
            if (player.photonView.OwnerActorNr == actorNumber)
            {
                return player;
            }
        }
        return null;
    }

    #endregion
}

/// <summary>
/// Record of a single bet for undo functionality
/// </summary>
[System.Serializable]
public class BetRecord
{
    public int BetSpaceIndex;
    public float Amount;
}
