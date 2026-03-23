using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;

/// <summary>
/// Player entry in the in-game player list - shows name, balance, and ready status
/// </summary>
public class PlayerEntryUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text betText;
    [SerializeField] private Image avatarImage;
    [SerializeField] private Image background;
    [SerializeField] private Image readyIndicator;
    [SerializeField] private GameObject masterIcon;

    [Header("Colors")]
    [SerializeField] private Color localPlayerColor = new Color(0.2f, 0.6f, 0.2f, 0.3f);
    [SerializeField] private Color otherPlayerColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
    [SerializeField] private Color readyColor = Color.green;
    [SerializeField] private Color notReadyColor = Color.gray;

    public Player Player { get; private set; }
    public bool IsLocalPlayer { get; private set; }

    /// <summary>
    /// Setup the entry with player data
    /// </summary>
    public void Setup(Player player)
    {
        Player = player;
        IsLocalPlayer = player.IsLocal;

        // Set name
        if (playerNameText != null)
        {
            playerNameText.text = player.NickName;
            if (string.IsNullOrEmpty(player.NickName))
            {
                playerNameText.text = $"Player_{player.ActorNumber}";
            }
        }

        // Set background color for local player
        if (background != null)
        {
            background.color = IsLocalPlayer ? localPlayerColor : otherPlayerColor;
        }

        // Show master icon
        if (masterIcon != null)
        {
            masterIcon.SetActive(player.IsMasterClient);
        }

        // Set initial balance (would be updated by game)
        UpdateBalance(3000f);
        UpdateBet(0f);
    }

    /// <summary>
    /// Update the displayed balance
    /// </summary>
    public void UpdateBalance(float balance)
    {
        if (balanceText != null)
        {
            balanceText.text = $"${balance:N0}";
        }
    }

    /// <summary>
    /// Update the current bet amount
    /// </summary>
    public void UpdateBet(float bet)
    {
        if (betText != null)
        {
            betText.text = bet > 0 ? $"Bet: ${bet:N0}" : "";
        }
    }

    /// <summary>
    /// Update ready status indicator
    /// </summary>
    public void UpdateReadyStatus(bool isReady)
    {
        if (readyIndicator != null)
        {
            readyIndicator.color = isReady ? readyColor : notReadyColor;
        }
    }

    /// <summary>
    /// Highlight this player (e.g., when they win)
    /// </summary>
    public void Highlight(bool highlight)
    {
        if (background != null)
        {
            Color color = highlight ? Color.yellow : (IsLocalPlayer ? localPlayerColor : otherPlayerColor);
            background.color = color;
        }
    }
}
