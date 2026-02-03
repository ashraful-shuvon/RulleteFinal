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
        BetPool.Instance.ResetStatus();
        SceneRoulette._Instance.camCtrl?.GoToOrigin();

        print(totalWin + " with " + result);
        bool win = totalWin > 0;
        if (win && ResultManager.totalBet > 0)
        {
            winPanel.SetActive(true);
            winText.text = string.Format("<color=#yellow>WIN</color> {0}", totalWin.ToString("F2"));
            AudioManager.SoundPlay(0);
        }

        string sRes;

        if (result != -1 && result != 37)
            sRes = result.ToString();
        else
            sRes = "00";

        resultText.text = sRes;

        bool isRed = false;

        for (int i = 0; i < redNumbers.Length; i++)
        {
            if (redNumbers[i] == result)
            {
                isRed = true;
                break;
            }
        }


        GameObject hOb = Instantiate(historyPrefab, historyContent);
        hOb.transform.SetAsFirstSibling();

        if (historyContent.childCount > 15)
            Destroy(historyContent.GetChild(15).gameObject);

        if (isRed)
        {
            hOb.transform.GetChild(1).GetComponent<TMP_Text>().text = sRes;
            resultText.color = Color.red;
        }
        else
        {
            TMP_Text blackHistoryText = hOb.transform.GetChild(0).GetComponent<TMP_Text>();

            if (sRes.Equals("0") || sRes.Equals("00"))
            {
                blackHistoryText.color = Color.green;
                resultText.color = Color.green;
            }
            else {
                blackHistoryText.color = Color.white;
                resultText.color = Color.white;
            }
            
            blackHistoryText.text = sRes;
        }
        StartCoroutine(EnableBets());
    }

    private IEnumerator EnableBets() {
        yield return new WaitForSecondsRealtime(2);
        BetSpace.EnableBets(true);
        SceneRoulette.GameStarted = false;
        SceneRoulette._Instance.rebetButton.gameObject.SetActive(true);
        winPanel.SetActive(false);
    }
}


