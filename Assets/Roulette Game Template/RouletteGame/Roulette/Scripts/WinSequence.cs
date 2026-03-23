using UnityEngine;
using TMPro;
using System.Collections;

public class WinSequence : MonoBehaviour {

    private readonly byte[] redNumbers = new byte[] { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
    public GameObject winPanel;
    public TMP_Text winText;
    
    public TMP_Text resultText;

    public GameObject historyPrefab;
    public Transform historyContent;
    
    public void ShowResult(int result, float totalWin)
    {
        // Plumbed to GameHUDController directly via ResultManager now
    }
}


