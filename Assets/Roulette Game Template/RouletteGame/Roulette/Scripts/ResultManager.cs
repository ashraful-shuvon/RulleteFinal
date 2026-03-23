using UnityEngine;
using System.Collections.Generic;

public class ResultManager : MonoBehaviour {

    private static ResultManager Instance;

    [SerializeField] private List<BetSpace> betSpaces;
    public static float totalBet = 0;
    public WinSequence winSequience;

    private void Awake()
    {
        Instance = this;
        Instance.betSpaces = new List<BetSpace>();
    }

    public static void SetResult(int result)
    {
        float totalWin = 0;
        
        foreach (BetSpace betSpace in Instance.betSpaces)
        {
            totalWin += betSpace.ResolveBet(result);
        }

        var hud = FindObjectOfType<GameHUDController>();
        if (hud != null)
        {
            hud.ShowResult(result, totalWin, totalWin > 0);
        }

        if (totalWin > 0)
        {
            AudioManager.SoundPlay(0);
        }

        totalBet = 0;
        ChipManager.EnableChips(true);
    }

    public static void ClearResult(int result)
    {
        GameObject previousResultHighlight = GameObject.Find("/_BACKGROUND_/ClothOb/Cloth/numberbets/high" + result.ToString());
        if (previousResultHighlight != null) { 
            previousResultHighlight.GetComponent<MeshRenderer>().enabled = false;
        }
    }
    
    public static void RegisterBetSpace(BetSpace betSpace)
    {
        Instance.betSpaces.Add(betSpace);
    }
}