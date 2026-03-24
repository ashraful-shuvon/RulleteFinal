using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    public static IAPManager Instance;

    private IStoreController storeController;
    private IExtensionProvider storeExtensionProvider;

    public Action OnPurchaseSuccessful;
    public Action<string> OnPurchaseFailedEvent;
    public Action OnInitializeSuccessful;

    [Header("Available Packages")]
    public IAPPackage[] packages;

    [Header("UI Fallback")]
    public GameObject iapPanel;
    public Transform packageContainer;
    public GameObject packageButtonPrefab;
    public Button closeButton;
    private bool openedFromWarning = false;

    private bool isInitialized = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        OverridePackages();

        if (storeController == null)
        {
            InitializePurchasing();
        }

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

    private void OverridePackages()
    {
        Sprite defaultIcon = packages != null && packages.Length > 0 ? packages[0].icon : null;
        
        packages = new IAPPackage[]
        {
            new IAPPackage { packageName = "50K CHIPS", chipAmount = 50000, price = 2.99f, productID = "com.casino.roulette.50k", icon = defaultIcon },
            new IAPPackage { packageName = "150K CHIPS", chipAmount = 150000, price = 4.99f, productID = "com.casino.roulette.150k", icon = defaultIcon },
            new IAPPackage { packageName = "300K CHIPS", chipAmount = 300000, price = 7.99f, productID = "com.casino.roulette.300k", icon = defaultIcon }
        };
    }

    public void InitializePurchasing()
    {
        if (IsInitialized()) return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        foreach (var package in packages)
        {
            if (!string.IsNullOrEmpty(package.productID))
            {
                builder.AddProduct(package.productID, ProductType.Consumable);
            }
        }

        UnityPurchasing.Initialize(this, builder);
    }

    public bool IsInitialized()
    {
        return storeController != null && storeExtensionProvider != null;
    }

    public void PurchasePackage(IAPPackage package)
    {
        BuyProductID(package.productID);
    }

    private void BuyProductID(string productId)
    {
        if (IsInitialized())
        {
            Product product = storeController.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                Debug.Log($"Purchasing product asynchronously: '{product.definition.id}'");
                storeController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase.");
                OnPurchaseFailedEvent?.Invoke("Product not available");
            }
        }
        else
        {
            Debug.Log("BuyProductID FAIL. Not initialized.");
            OnPurchaseFailedEvent?.Invoke("Purchasing not initialized");
        }
    }
    
    // Restore Purchases (Required for iOS)
    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log("RestorePurchases started ...");

            var apple = storeExtensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((result, error) => {
                Debug.Log($"RestorePurchases continuing: {result}. If no further messages, no purchases available to restore.");
            });
        }
        else
        {
            Debug.Log("RestorePurchases FAIL. Not supported on this platform.");
        }
    }

    // --- IStoreListener Methods ---

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized: PASS");
        storeController = controller;
        storeExtensionProvider = extensions;
        isInitialized = true;
        OnInitializeSuccessful?.Invoke();
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log($"OnInitializeFailed InitializationFailureReason:{error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log($"OnInitializeFailed InitializationFailureReason:{error} Message:{message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        // Find which package was purchased
        foreach (var package in packages)
        {
            if (string.Equals(args.purchasedProduct.definition.id, package.productID, StringComparison.Ordinal))
            {
                Debug.Log($"ProcessPurchase: PASS. Product: '{args.purchasedProduct.definition.id}'");
                
                // Add balance to networked player safely if playing natively
                if (NetworkPlayer.LocalPlayer != null)
                {
                    NetworkPlayer.LocalPlayer.ReceiveWinnings(package.chipAmount); 
                }
                else
                {
                    // Fallback to offline balance manager
                    BalanceManager.ChangeBalance(package.chipAmount);
                }

                AudioManager.SoundPlay(3); // Success sound

                if (SceneRoulette._Instance != null)
                {
                    SceneRoulette._Instance.ShowPurchaseSuccess(package.chipAmount);
                }

                CloseIAPPanel();
                OnPurchaseSuccessful?.Invoke();
                return PurchaseProcessingResult.Complete;
            }
        }

        Debug.Log($"ProcessPurchase: FAIL. Unrecognized product: '{args.purchasedProduct.definition.id}'");
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log($"OnPurchaseFailed: FAIL. Product: '{product.definition.storeSpecificId}', PurchaseFailureReason: {failureReason}");
        OnPurchaseFailedEvent?.Invoke(failureReason.ToString());
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.Log($"OnPurchaseFailed: FAIL. Product: '{product.definition.storeSpecificId}', Description: {failureDescription.message}");
        OnPurchaseFailedEvent?.Invoke(failureDescription.message);
    }
    
    public string GetLocalizedPriceString(string productId)
    {
        if (IsInitialized())
        {
            Product p = storeController.products.WithID(productId);
            if (p != null)
                return p.metadata.localizedPriceString;
        }
        return null; // Return null to fallback to static inspector price
    }

    // --- Legacy UI ---
    public void OpenIAPPanel()
    {
        openedFromWarning = false;
        if (iapPanel != null)
        {
            iapPanel.SetActive(true);
            AudioManager.SoundPlay(3);
        }
    }

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
    public Sprite backgroundSprite;  
    public Sprite icon;              
    public Color buttonColor = Color.white;  

    public string GetPriceString()
    {
        if (IAPManager.Instance != null && IAPManager.Instance.IsInitialized())
        {
            string locPrice = IAPManager.Instance.GetLocalizedPriceString(productID);
            if (!string.IsNullOrEmpty(locPrice)) return locPrice;
        }
        return $"${price:F2}";
    }
}