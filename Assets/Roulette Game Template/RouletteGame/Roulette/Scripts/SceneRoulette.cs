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


    void Awake()
    {
        _Instance = this;
    }

    private void Start()
    {
        BalanceManager.SetBalance(1000);
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

    
}

