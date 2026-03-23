using UnityEngine;
using Photon.Pun;
using System.Collections;

/// <summary>
/// Networked version of the roulette wheel - synchronizes spin results across all clients
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class NetworkedWheel : MonoBehaviourPun
{
    public static NetworkedWheel Instance;

    [Header("Wheel References")]
    [SerializeField] private EuropeanWheel europeanWheel;
    [SerializeField] private AmericanWheel americanWheel;
    [SerializeField] private BallManager ballManager;

    [Header("Spin Settings")]
    [SerializeField] private float spinDelayBeforeResult = 5f;

    [Header("Visual References")]
    [SerializeField] private Transform wheelTransform;
    [SerializeField] private ParticleSystem spinParticles;

    // State
    public bool IsSpinning { get; private set; }
    public int CurrentResult { get; private set; } = -1;
    public bool IsEuropean { get; private set; } = true;

    // Events
    public System.Action<int> OnSpinComplete;
    public System.Action OnSpinStart;

    private Coroutine spinCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Find wheel references if not assigned
        if (europeanWheel == null)
            europeanWheel = GetComponent<EuropeanWheel>();
        if (americanWheel == null)
            americanWheel = GetComponent<AmericanWheel>();
        if (ballManager == null)
            ballManager = GetComponent<BallManager>();

        // Determine wheel type
        IsEuropean = europeanWheel != null && europeanWheel.enabled;
    }

    private void Start()
    {
        // Register with NetworkGameState
        if (NetworkGameState.Instance != null)
        {
            NetworkGameState.Instance.OnSpinResultReceived += OnSpinResultReceived;
        }
    }

    private void OnDestroy()
    {
        if (NetworkGameState.Instance != null)
        {
            NetworkGameState.Instance.OnSpinResultReceived -= OnSpinResultReceived;
        }
    }

    #region Spin Control

    /// <summary>
    /// Called by NetworkGameState when spin result is received
    /// </summary>
    private void OnSpinResultReceived(int result)
    {
        Debug.Log($"[NetworkedWheel] Received spin result: {result}");

        CurrentResult = result;
        IsSpinning = true;

        // Start the visual spin
        if (spinCoroutine != null)
            StopCoroutine(spinCoroutine);

        spinCoroutine = StartCoroutine(SpinSequence(result));
    }

    /// <summary>
    /// Execute the spin sequence with synchronized result
    /// </summary>
    private IEnumerator SpinSequence(int result)
    {
        OnSpinStart?.Invoke();

        Debug.Log($"[NetworkedWheel] Starting spin sequence for result: {result}");

        // Trigger spin particles if available
        if (spinParticles != null)
        {
            spinParticles.Play();
        }

        // Start the ball spin
        if (ballManager != null)
        {
            ballManager.StartSpin();
        }

        // Enable wheel rotation via the wheel's Spin method or direct invocation
        // Since 'spinning' is protected, we trigger the spin through the public method
        if (IsEuropean && europeanWheel != null)
        {
            // Call the base Spin which sets spinning = true and starts the ball
            europeanWheel.Spin();
        }
        else if (americanWheel != null)
        {
            americanWheel.Spin();
        }

        // Wait before showing result
        yield return new WaitForSeconds(spinDelayBeforeResult);

        // Direct ball to result position
        if (ballManager != null)
        {
            ballManager.FindNumber(result, IsEuropean);
        }

        // Disable spinning after result
        IsSpinning = false;

        Debug.Log($"[NetworkedWheel] Spin complete. Result: {result}");
        OnSpinComplete?.Invoke(result);
    }

    /// <summary>
    /// Force stop spinning (for cleanup)
    /// </summary>
    public void StopSpin()
    {
        if (spinCoroutine != null)
        {
            StopCoroutine(spinCoroutine);
            spinCoroutine = null;
        }

        IsSpinning = false;

        // Note: Cannot directly set spinning=false as it's protected
        // The wheel will stop naturally when the spin sequence ends
    }

    #endregion

    #region RPCs for Synchronization

    /// <summary>
    /// Sync spin start to all clients (called by master client)
    /// </summary>
    [PunRPC]
    private void RpcStartSpin(int result)
    {
        Debug.Log($"[NetworkedWheel] RpcStartSpin received with result: {result}");

        CurrentResult = result;
        IsSpinning = true;

        if (spinCoroutine != null)
            StopCoroutine(spinCoroutine);

        spinCoroutine = StartCoroutine(SpinSequence(result));
    }

    /// <summary>
    /// Call this to broadcast spin to all clients (Master Client only)
    /// </summary>
    public void BroadcastSpin(int result)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[NetworkedWheel] Only Master Client can broadcast spin");
            return;
        }

        photonView.RPC("RpcStartSpin", RpcTarget.All, result);
    }

    #endregion

    #region Utility

    /// <summary>
    /// Get the wheel type as string
    /// </summary>
    public string GetWheelTypeString()
    {
        return IsEuropean ? "European" : "American";
    }

    /// <summary>
    /// Check if wheel is available for spin
    /// </summary>
    public bool CanSpin()
    {
        return !IsSpinning && NetworkGameState.Instance?.CurrentPhase == GamePhase.Spinning;
    }

    #endregion
}
