using UnityEngine;
using UnityEngine.UI;

public class IAPManager : MonoBehaviour
{
    public static IAPManager Instance;

    [Header("IAP Packages")]
    public IAPPackage[] packages;

    [Header("UI")]
    public GameObject iapPanel;
    public Transform packageContainer;
    public GameObject packageButtonPrefab;
    public Button closeButton;

    private bool openedFromWarning = false; // Track if opened from warning

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (iapPanel != null)
            iapPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseIAPPanel);

        CreatePackageButtons();
    }

    void CreatePackageButtons()
    {
        if (packageContainer == null || packageButtonPrefab == null)
            return;

        foreach (IAPPackage package in packages)
        {
            GameObject btn = Instantiate(packageButtonPrefab, packageContainer);

            IAPPackageButton btnScript = btn.GetComponent<IAPPackageButton>();
            if (btnScript != null)
            {
                btnScript.Setup(package);
            }
        }
    }

    // Called when purchase is successful
    public void PurchasePackage(IAPPackage package)
    {
        // Add balance to game
        BalanceManager.ChangeBalance(package.chipAmount);

        AudioManager.SoundPlay(3); // Success sound

        // Close IAP panel
        CloseIAPPanel();

        // Show success message
        if (SceneRoulette._Instance != null)
        {
            // Show success in warning panel style
            SceneRoulette._Instance.ShowPurchaseSuccess(package.chipAmount);
        }
    }

    // Open from main UI (normal shop access)
    public void OpenIAPPanel()
    {
        openedFromWarning = false;

        if (iapPanel != null)
        {
            iapPanel.SetActive(true);
            AudioManager.SoundPlay(3);
        }
    }

    // Open from warning panel (when balance insufficient)
    public void OpenIAPPanelFromWarning()
    {
        openedFromWarning = true;

        if (iapPanel != null)
        {
            iapPanel.SetActive(true);
            AudioManager.SoundPlay(3);
        }
    }

    public void CloseIAPPanel()
    {
        if (iapPanel != null)
        {
            iapPanel.SetActive(false);
            AudioManager.SoundPlay(3);
        }

        // If was opened from warning, user can continue playing
        // Warning is already closed, so player returns to game
        openedFromWarning = false;
    }
}

[System.Serializable]
public class IAPPackage
{
    public string packageName;
    public float chipAmount;
    public float price;
    public string productID;

    [Header("Visuals")]
    public Sprite backgroundSprite;  // NEW: Custom background image
    public Sprite icon;              // Optional icon
    public Color buttonColor = Color.white;  // Fallback if no sprite
}