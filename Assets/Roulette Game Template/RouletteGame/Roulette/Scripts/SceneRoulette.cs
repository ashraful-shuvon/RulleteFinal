using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System;

public class SceneRoulette : MonoBehaviour
{
    public static SceneRoulette _Instance;
    
    public static int uiState = 0;  // popup window shows or not

    public BetPool pool;
    public EuropeanWheel _EuroWheel;    // slot game clase
    public AmericanWheel _AmeWheel;     // slot game clase

    [Space]
    [Header("Text")]
    public TMP_Text textBalance;        // user balance info
    public TMP_Text textBet;            // user bet info
    public TMP_Text resultText;            // result info

    [Space]
    [Header("UI")]
    public Button clearButton;
    public Button undoButton;
    public Button rebetButton;
    public Button rollButton;

    public Slider volumeSlider;
    public Toggle soundToggle;
    public Toggle musicToggle;

    [Space]
    [Header("Extra")]
    public CameraController camCtrl;
    public static float WaitTime;
    public static bool GameStarted = false;
    public static bool MenuOn = false;

    // Warning messages
    [Space]
    [Header("Warning Messages")]
    public GameObject warningPanel;         // Add this
    public TMP_Text warningText;            // Add this
    public Button warningCloseButton;       // Add this
    public Button addBalanceButton;
    public Button buyChipsButton;

    void Awake()
    {
        _Instance = this;
    }

    private void Start()
    {
        BalanceManager.SetBalance(3000);

        // Setup buy chips button in warning panel
        if (buyChipsButton != null)
        {
            buyChipsButton.onClick.AddListener(OpenShopFromWarning);
        }
    }

    public void MessageQuitResult(int value)
    {
        if (value == 0)
        {
            Application.Quit();
        }
    }
    
    public void OnButtonClear()
    {
        AudioManager.SoundPlay(3);
        clearButton.interactable = false;
        rollButton.interactable = false;
        pool.Clear();
    }

    public void OnButtonUndo()
    {
        undoButton.interactable = false;
        AudioManager.SoundPlay(3);
        pool.Undo();
    }

    public void OnButtonRebet()
    {
        rebetButton.gameObject.SetActive(false);
        StartCoroutine(pool.Rebet());
    }

    public void OnButtonRoll()
    {
        undoButton.interactable = false;
        clearButton.interactable = false;
        rollButton.interactable = false;
        resultText.text = "";
        SpinRoulette();
    }

    public void SpinRoulette()
    {
        
        if (_EuroWheel != null)
            _EuroWheel.Spin();
        else if(_AmeWheel != null)
            _AmeWheel.Spin();

        ChangeUI();
        AudioManager.SoundPlay(2);
    }

    public void ChangeUI()
    {
        if(camCtrl != null)
            camCtrl.GoToTarget();
        ToolTipManager.Deselect();
        clearButton.interactable = false;
        undoButton.interactable = false;
        rebetButton.gameObject.SetActive(false);
        rollButton.interactable = false;
        ChipManager.EnableChips(false);
    }

    public void BlockBets()
    {
        MenuOn = true;
        BetSpace.EnableBets(false);
    }

    public void ReleaseBets()
    {
        MenuOn = false;
        BetSpace.EnableBets(!GameStarted);
    }

    public static void UpdateLocalPlayerText()
    {
        _Instance.textBet.text = "Bet: " + ResultManager.totalBet.ToString("F2");
        _Instance.textBalance.text = BalanceManager.Balance.ToString("F2");
    }

    
    
    // IAP Shop method called from warning panel - opens shop and closes warning panel

    // Existing ShowWarning with both buttons
    public static void ShowWarning(string message, bool showBalanceButtons)
    {
        if (_Instance != null)
        {
            _Instance.warningText.text = message;
            _Instance.warningPanel.SetActive(true);

            // Show/hide add balance button
            if (_Instance.addBalanceButton != null)
            {
                _Instance.addBalanceButton.gameObject.SetActive(showBalanceButtons);
            }

            // Show/hide buy chips button
            if (_Instance.buyChipsButton != null)
            {
                _Instance.buyChipsButton.gameObject.SetActive(showBalanceButtons);
            }

            AudioManager.SoundPlay(4);
        }
    }

    // Keep existing AddBalance function
    public void AddBalance()
    {
        BalanceManager.ChangeBalance(1000);
        AudioManager.SoundPlay(3);

        warningText.text = "+$1000 Added!";
        warningText.color = Color.green;

        if (addBalanceButton != null)
        {
            addBalanceButton.gameObject.SetActive(false);
        }

        if (buyChipsButton != null)
        {
            buyChipsButton.gameObject.SetActive(false);
        }

        StartCoroutine(CloseWarningAfterDelay(1.5f));
    }

    private IEnumerator CloseWarningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseWarning();

        if (warningText != null)
        {
            warningText.color = Color.white;
        }
    }

    public void CloseWarning()
    {
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
    }

    // NEW: Open shop from warning panel (closes warning first)
    public void OpenShopFromWarning()
    {
        // Close warning panel
        CloseWarning();

        // Open IAP shop
        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.OpenIAPPanelFromWarning();
        }
    }

    // NEW: Open shop from main UI
    public void OpenShop()
    {
        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.OpenIAPPanel();
        }
    }

    // NEW: Show purchase success message
    public void ShowPurchaseSuccess(float amount)
    {
        if (warningPanel != null && warningText != null)
        {
            warningText.text = $"Purchase Successful!\n+${amount:F0} Chips Added!";
            warningText.color = Color.green;
            warningPanel.SetActive(true);

            // Hide buttons
            if (addBalanceButton != null)
                addBalanceButton.gameObject.SetActive(false);
            if (buyChipsButton != null)
                buyChipsButton.gameObject.SetActive(false);

            // Auto close after 2 seconds
            StartCoroutine(CloseWarningAfterDelay(2f));
        }
    }


    /*--------------------------------------------------------------------------------------------*/
    // Warning message methods

    /*
    public static void ShowWarning(string message)
    {
        if (_Instance != null)
        {
            _Instance.warningText.text = message;
            _Instance.warningPanel.SetActive(true);
            AudioManager.SoundPlay(4); // Error sound
        }
    }
    */

    // ADD THESE NEW FUNCTIONS - Don't change anything else

    // Overloaded version with button control

    /* Updated on 2026-02-18: Added button control to ShowWarning method and implemented AddBalance method with confirmation message.
    public static void ShowWarning(string message, bool showAddBalanceButton)
    {
        if (_Instance != null)
        {
            _Instance.warningText.text = message;
            _Instance.warningPanel.SetActive(true);

            // Show/hide the add balance button
            if (_Instance.addBalanceButton != null)
            {
                _Instance.addBalanceButton.gameObject.SetActive(showAddBalanceButton);
            }

            AudioManager.SoundPlay(4);
        }
    }

    
    public void AddBalance()
    {
        BalanceManager.ChangeBalance(1000); // Add $1000
        AudioManager.SoundPlay(3); // Success sound
        CloseWarning();

        // Optional: Show confirmation
        if (resultText != null)
        {
            resultText.text = "+$1000 Added!";
            resultText.color = Color.green;
            StartCoroutine(ClearResultTextAfterDelay(2f));
        }
    }

    private IEnumerator ClearResultTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (resultText != null)
        {
            resultText.text = "";
            resultText.color = Color.white;
        }
    }
    

    public void AddBalance()
    {
        BalanceManager.ChangeBalance(1000);
        AudioManager.SoundPlay(3);

        // Show success message in warning panel
        warningText.text = "+$1000 Added!";
        warningText.color = Color.green;

        // Hide the add balance button
        if (addBalanceButton != null)
        {
            addBalanceButton.gameObject.SetActive(false);
        }

        // Close after a short delay
        StartCoroutine(CloseWarningAfterDelay(1.5f));
    }

    private IEnumerator CloseWarningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseWarning();

        // Reset color back to white
        if (warningText != null)
        {
            warningText.color = Color.white;
        }
    }

    public void CloseWarning()
    {
        warningPanel.SetActive(false);
    }

    */

}

