using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Chip : MonoBehaviour {
    
    public float value;
    
    public GameObject ring;

    private void Select()
    {
        // Handled by UI Toolkit now
    }

    public void OnClick()
    {
        // Handled by UI Toolkit now
    }

    public void OnPointEnter()
    {
        if (BetSpace.BetsEnabled)
        {
            transform.DOComplete();
            transform.DOScale(1.2f, .3f);
        }
    }

    public void OnPointExit()
    {
        if (BetSpace.BetsEnabled)
        {
            transform.DOComplete();
            transform.DOScale(1f, .2f);
        }
    }
}
