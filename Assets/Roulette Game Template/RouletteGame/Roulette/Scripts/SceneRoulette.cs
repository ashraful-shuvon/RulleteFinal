using UnityEngine;
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
    [Header("Extra")]
    public CameraController camCtrl;
    public static float WaitTime;
    public static bool GameStarted = false;
    public static bool MenuOn = false;

    void Awake()
    {
        _Instance = this;
    }

    private void Start()
    {
    }

    public void MessageQuitResult(int value)
    {
        if (value == 0)
        {
            Application.Quit();
        }
    }

    public void ChangeUI()
    {
        if(camCtrl != null)
            camCtrl.GoToTarget();
        ToolTipManager.Deselect();
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

    // Deprecated methods kept empty for compatibility if any other script calls them
    public static void UpdateLocalPlayerText() { }
    public static void ShowWarning(string message, bool showBalanceButtons) { }
    public void AddBalance() { }
    public void CloseWarning() { }
    public void OpenShopFromWarning() { }
    public void OpenShop() { }
    public void ShowPurchaseSuccess(float amount) { }
}

