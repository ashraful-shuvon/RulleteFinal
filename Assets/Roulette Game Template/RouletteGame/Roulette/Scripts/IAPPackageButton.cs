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
            priceText.text = pkg.GetPriceString();

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

        // Set background sprite or color
        if (backgroundImage != null)
        {
            if (pkg.backgroundSprite != null)
            {
                // Use custom sprite background
                backgroundImage.sprite = pkg.backgroundSprite;
                backgroundImage.color = Color.white; // Full opacity to show sprite
                backgroundImage.type = Image.Type.Sliced; // For proper scaling
            }
            else
            {
                // Fallback to solid color if no sprite provided
                backgroundImage.sprite = null;
                backgroundImage.color = pkg.buttonColor;
                backgroundImage.type = Image.Type.Simple;
            }
        }
    }

    void OnPurchaseClick()
    {
        if (IAPManager.Instance != null)
        {
            Debug.Log($"Purchasing: {package.packageName} - ${package.chipAmount} chips for ${package.price}");
            IAPManager.Instance.PurchasePackage(package);
        }
    }
}