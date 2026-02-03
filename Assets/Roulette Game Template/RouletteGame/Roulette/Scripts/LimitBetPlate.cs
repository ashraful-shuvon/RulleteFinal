using DG.Tweening;
using UnityEngine;
using TMPro;

public class LimitBetPlate : MonoBehaviour
{
    public static float max = 1000f;
    public static float min = 1f;
    public SpriteRenderer plate;

    public TMP_Text minT;
    public TMP_Text maxT;

    public static LimitBetPlate Instance;

    private void Start()
    {
        Instance = this;
        UpdateText();
    }

    public static void UpdateText()
    {
        Instance.minT.text = string.Format("<color=red>{0}</color> {1}", "Min", min.ToString("0.0"));
        Instance.maxT.text = string.Format("<color=red>{0}</color> {1}", "Max", max.ToString("0.0"));
    }

    public static void SetBetLimits(float minBet, float maxBet)
    {
        min = minBet;
        max = maxBet;
    }

    public static bool AllowLimit(float value)
    {
        if (ResultManager.totalBet + value > max || ResultManager.totalBet + value < min)
        {
            Instance.plate.color = new Color(1,1,1,0);
            DOTween.Sequence().Append(Instance.plate.DOFade(1, .3f).SetEase(Ease.OutBounce)).Append(Instance.plate.DOFade(0, .3f));
            return false;
        }
        return true;
    }
}
