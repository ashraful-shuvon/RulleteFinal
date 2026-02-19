using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IAPPackageButton : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text packageNameText;
    public TMP_Text chipsText;
    public TMP_Text priceText;
    public Image iconImage;
    public Image backgroundImage;

    private IAPPackage package;
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnPurchaseClick);
        }
    }

    public void Setup(IAPPackage pkg)
    {
        package = pkg;

        // Set package name
        if (packageNameText != null)
            packageNameText.text = pkg.packageName;

        // Set chips amount
        if (chipsText != null)
            chipsText.text = $"${pkg.chipAmount:F0} CHIPS";

        // Set price
        if (priceText != null)
            priceText.text = $"${pkg.price:F2}";

        // Set icon (optional)
        if (iconImage != null && pkg.icon != null)
        {
            iconImage.sprite = pkg.icon;
            iconImage.gameObject.SetActive(true);
        }
        else if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }

        // Set background color
        if (backgroundImage != null)
            backgroundImage.color = pkg.buttonColor;
    }

    void OnPurchaseClick()
    {
        // For testing: instant purchase (no real money involved)
        if (IAPManager.Instance != null)
        {
            Debug.Log($"Purchasing: {package.packageName} - ${package.chipAmount} chips for ${package.price}");
            IAPManager.Instance.PurchasePackage(package);
        }

        // TODO: For real IAP integration, replace above with:
        // IAPService.PurchaseProduct(package.productID, OnPurchaseSuccess, OnPurchaseFailed);
    }

    // For future real IAP integration
    void OnPurchaseSuccess(string productID)
    {
        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.PurchasePackage(package);
        }
    }

    void OnPurchaseFailed(string error)
    {
        Debug.LogError($"Purchase failed: {error}");
        SceneRoulette.ShowWarning($"Purchase Failed!\n{error}", false);
    }
}