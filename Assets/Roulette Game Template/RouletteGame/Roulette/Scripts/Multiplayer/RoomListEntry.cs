using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;

/// <summary>
/// Individual room entry in the room list - displays room info and join button
/// </summary>
public class RoomListEntry : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text stakesText;
    [SerializeField] private Image tableTypeIcon;
    [SerializeField] private Button joinButton;

    [Header("Colors")]
    [SerializeField] private Color lowStakesColor = Color.green;
    [SerializeField] private Color mediumStakesColor = Color.yellow;
    [SerializeField] private Color highStakesColor = Color.red;

    private RoomInfo roomInfo;
    private System.Action<string> onJoinClicked;

    private void Awake()
    {
        if (joinButton != null)
        {
            joinButton.onClick.AddListener(OnJoinButtonClicked);
        }
    }

    /// <summary>
    /// Setup the room entry with data
    /// </summary>
    public void Setup(RoomInfo room, System.Action<string> joinCallback)
    {
        roomInfo = room;
        onJoinClicked = joinCallback;

        // Set room name
        if (roomNameText != null)
        {
            roomNameText.text = room.Name;
        }

        // Set player count
        if (playerCountText != null)
        {
            playerCountText.text = $"{room.PlayerCount}/{room.MaxPlayers}";
        }

        // Set stakes info
        if (room.CustomProperties.TryGetValue("TableStakes", out object stakesObj))
        {
            TableStakes stakes = (TableStakes)(int)stakesObj;
            SetStakesInfo(stakes);

            // Get min/max bet
            if (room.CustomProperties.TryGetValue("MinBet", out object minBet) &&
                room.CustomProperties.TryGetValue("MaxBet", out object maxBet))
            {
                if (stakesText != null)
                {
                    stakesText.text = $"${minBet} - ${maxBet}";
                }
            }
        }

        // Disable join if room is full
        if (joinButton != null)
        {
            joinButton.interactable = room.PlayerCount < room.MaxPlayers;
        }
    }

    private void SetStakesInfo(TableStakes stakes)
    {
        Color stakesColor = stakes switch
        {
            TableStakes.Low => lowStakesColor,
            TableStakes.Medium => mediumStakesColor,
            TableStakes.High => highStakesColor,
            _ => Color.white
        };

        if (tableTypeIcon != null)
        {
            tableTypeIcon.color = stakesColor;
        }

        // Update stakes label color
        if (stakesText != null)
        {
            stakesText.color = stakesColor;
        }
    }

    private void OnJoinButtonClicked()
    {
        if (roomInfo != null && onJoinClicked != null)
        {
            onJoinClicked(roomInfo.Name);
        }
    }

    /// <summary>
    /// Get the room name
    /// </summary>
    public string RoomName => roomInfo?.Name ?? "";

    /// <summary>
    /// Check if room is joinable
    /// </summary>
    public bool CanJoin => roomInfo != null && roomInfo.PlayerCount < roomInfo.MaxPlayers;
}
