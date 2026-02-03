using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Chip : MonoBehaviour {
    
    public float value;
    
    public GameObject ring;

    private void Select()
    {
        ChipManager.selected = this;
        ChipManager.selected.ring.SetActive(true);
    }

    public void OnClick()
    {
        if (!BetSpace.BetsEnabled)
            return;

        transform.DOComplete();
        if (ChipManager.selected)
        {
            AudioManager.SoundPlay(3);
            ChipManager.selected.transform.DOScale(1f, .2f);
            ChipManager.selected.ring.SetActive(false);
        }
        transform.DOShakeScale(.3f, .2f, 10, 0);
        Select();
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
