using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;

/// <summary>
/// Handles initial Photon connection and player name input
/// Attach this to the Launcher/Authentication scene
/// </summary>
public class LauncherUI : MonoBehaviourPunCallbacks
{
    public static LauncherUI Instance;

    [Header("Connection Panels")]
    [SerializeField] private GameObject connectionPanel;
    [SerializeField] private GameObject connectingPanel;
    [SerializeField] private GameObject errorPanel;

    [Header("Player Name")]
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button playButton;
    [SerializeField] private Button guestButton;
    [SerializeField] private TMP_Text placeholderText;

    [Header("Connecting")]
    [SerializeField] private TMP_Text connectingText;
    [SerializeField] private Slider loadingSlider;

    [Header("Error")]
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private Button retryButton;

    [Header("Scene Settings")]
    [SerializeField] private string lobbySceneName = "Lobby";

    [Header("Debug")]
    [SerializeField] private bool autoConnectOnStart = false;

    private bool isConnecting = false;

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
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);

        if (guestButton != null)
            guestButton.onClick.AddListener(OnGuestButtonClicked);

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryButtonClicked);

        // Show connection panel
        ShowConnectionPanel();

        // Auto-connect for debugging
        if (autoConnectOnStart)
        {
            ConnectWithRandomName();
        }
    }

    #region Panel Management

    private void ShowConnectionPanel()
    {
        if (connectionPanel != null) connectionPanel.SetActive(true);
        if (connectingPanel != null) connectingPanel.SetActive(false);
        if (errorPanel != null) errorPanel.SetActive(false);

        // Load saved player name
        string savedName = PlayerPrefs.GetString("PlayerName", "");
        if (!string.IsNullOrEmpty(savedName) && playerNameInput != null)
        {
            playerNameInput.text = savedName;
        }
    }

    private void ShowConnectingPanel(string message = "Connecting...")
    {
        if (connectionPanel != null) connectionPanel.SetActive(false);
        if (connectingPanel != null) connectingPanel.SetActive(true);
        if (errorPanel != null) errorPanel.SetActive(false);

        if (connectingText != null)
            connectingText.text = message;

        // Start loading animation
        if (loadingSlider != null)
            StartCoroutine(LoadingAnimation());
    }

    private void ShowErrorPanel(string errorMessage)
    {
        if (connectionPanel != null) connectionPanel.SetActive(false);
        if (connectingPanel != null) connectingPanel.SetActive(false);
        if (errorPanel != null) errorPanel.SetActive(true);

        if (errorText != null)
            errorText.text = errorMessage;
    }

    #endregion

    #region Button Handlers

    private void OnPlayButtonClicked()
    {
        string playerName = playerNameInput.text;

        if (string.IsNullOrEmpty(playerName) || playerName.Length < 2)
        {
            // Show error
            if (placeholderText != null)
            {
                placeholderText.text = "Please enter a name (min 2 characters)";
                placeholderText.color = Color.red;
            }
            return;
        }

        ConnectToPhoton(playerName);
    }

    private void OnGuestButtonClicked()
    {
        ConnectWithRandomName();
    }

    private void OnRetryButtonClicked()
    {
        ConnectToPhoton(PlayerPrefs.GetString("PlayerName", ""));
    }

    #endregion

    #region Connection

    private void ConnectToPhoton(string playerName)
    {
        if (isConnecting) return;

        isConnecting = true;

        // Save player name
        if (!string.IsNullOrEmpty(playerName))
        {
            PlayerPrefs.SetString("PlayerName", playerName);
            PhotonNetwork.NickName = playerName;
        }
        else
        {
            PhotonNetwork.NickName = $"Player_{Random.Range(1000, 9999)}";
        }

        ShowConnectingPanel("Connecting to Photon...");

        // Connect via NetworkManager
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.ConnectToPhoton();
        }
        else
        {
            // Fallback direct connection
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    private void ConnectWithRandomName()
    {
        string randomName = $"Player_{Random.Range(10000, 99999)}";
        ConnectToPhoton(randomName);
    }

    #endregion

    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("[LauncherUI] Connected to Master Server");

        if (connectingText != null)
            connectingText.text = "Joining Lobby...";

        // Join lobby
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("[LauncherUI] Joined Lobby");

        isConnecting = false;

        // Load lobby scene
        if (!string.IsNullOrEmpty(lobbySceneName))
        {
            SceneManager.LoadScene(lobbySceneName);
        }
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        Debug.LogWarning($"[LauncherUI] Disconnected: {cause}");

        isConnecting = false;

        string errorMessage = cause switch
        {
            Photon.Realtime.DisconnectCause.ServerTimeout => "Server timeout. Please check your connection.",
            Photon.Realtime.DisconnectCause.ClientTimeout => "Connection timeout. Please try again.",
            Photon.Realtime.DisconnectCause.InvalidRegion => "Invalid region selected.",
            Photon.Realtime.DisconnectCause.CustomAuthenticationFailed => "Authentication failed.",
            Photon.Realtime.DisconnectCause.MaxCcuReached => "Server is full. Please try again later.",
            _ => $"Connection lost: {cause}"
        };

        ShowErrorPanel(errorMessage);
    }

    public override void OnCustomAuthenticationFailed(string debugMessage)
    {
        Debug.LogError($"[LauncherUI] Auth failed: {debugMessage}");
        ShowErrorPanel($"Authentication failed: {debugMessage}");
    }

    #endregion

    #region Animations

    private System.Collections.IEnumerator LoadingAnimation()
    {
        float progress = 0f;
        while (connectingPanel != null && connectingPanel.activeSelf)
        {
            progress += Time.deltaTime * 0.5f;
            if (loadingSlider != null)
            {
                loadingSlider.value = Mathf.PingPong(progress, 1f);
            }
            yield return null;
        }
    }

    #endregion

    #region Utility

    /// <summary>
    /// Check if player is already authenticated (from PlayFab)
    /// </summary>
    public void CheckPlayFabAuthentication()
    {
        // This would integrate with PlayFab authentication
        // If already authenticated with PlayFab, use that identity
        Debug.Log("[LauncherUI] Checking PlayFab authentication...");
    }

    #endregion
}
