using UnityEngine;
using System.Collections;
using TMPro;

public class ToolTipManager : MonoBehaviour {

    public static ToolTipManager Instance;

    public GameObject toolTip;
    public TMP_Text toolTipText;

    [HideInInspector]
    public static ChipStack Target { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public static void SelectTarget(ChipStack stack)
    {
        Target = stack;

        if (Target)
        {
            Instance.toolTip.SetActive(true);

            Instance.toolTip.transform.position = Camera.main.WorldToScreenPoint(Target.transform.position);
            Instance.toolTipText.text = Target.GetValue().ToString();

        }
    }
    public static void Deselect()
    {
        Target = null;
        Instance.toolTip.SetActive(false);
    }
}
