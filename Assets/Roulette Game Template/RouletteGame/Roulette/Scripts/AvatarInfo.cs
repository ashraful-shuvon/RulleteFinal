using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class AvatarInfo : MonoBehaviour
{
    private Button avatarButton;

    public TMP_Text nameText;
    public TMP_Text creditsText;

    public Image timeRim;
    public Image Icon;
    public static float totalTime;
    public Image readySprite;
    public Image rim;

    private bool timerOn = false;

    private void Start()
    {
        avatarButton = GetComponent<Button>();
        readySprite.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (timerOn) {
            timeRim.fillAmount -= 1/totalTime * Time.deltaTime;
            if (timeRim.fillAmount <= 0)
            {
                timerOn = false;
            }
        }
    }

    public void SetCredits(int credits)
    {
        creditsText.text = "$" + credits.ToString();
    }

    public void FillInfo(string nickname, string balance, int avatarIndex)
    {
        ChangeAvatar(avatarIndex);
        nameText.text = nickname;
        creditsText.text = "$" + balance;
        SetActiveAvatarButton(false);
    }

    public void ShowReady()
    {
        readySprite.gameObject.SetActive(true);
        readySprite.color = new Color(0,0,0,.4f);
    }
    public void HideReady()
    {
        readySprite.gameObject.SetActive(false);
    }

    public void ResetInfo()
    {
        nameText.text = "NickName";
        creditsText.text = "$ -";
        SetActiveAvatarButton(true);
        readySprite.gameObject.SetActive(false);
        ToggleGoldRim(false);
        ChangeAvatar(1);
    }

    public void StartTimer(float time)
    {
        totalTime = time;
        timerOn = true;
        timeRim.fillAmount = 1;
    }

    public void StartTimer()
    {
        timerOn = true;
        timeRim.fillAmount = 1;
    }

    public void StopTimer()
    {
        timerOn = false;
        timeRim.fillAmount = 0;
    }

    public void SetActiveAvatarButton(bool active)
    {
        avatarButton.interactable = active;
    }

    public void ChangeAvatar(int index)
    {
        if (index < 0)
            index = 0;
        string avatarName = string.Format("Avatars/avatar{0}", index);
        Icon.sprite = Resources.Load<Sprite>(avatarName);
    }

    internal void ToggleGoldRim(bool on)
    {
        if(on)
            rim.sprite = Resources.Load<Sprite>("Avatars/playerRim");
        else
            rim.sprite = Resources.Load<Sprite>("Avatars/neutralRim");
    }
}
