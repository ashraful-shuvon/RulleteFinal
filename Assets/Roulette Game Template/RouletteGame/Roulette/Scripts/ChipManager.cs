using UnityEngine;

public class ChipManager : MonoBehaviour {

    public static Chip selected = null;
    private static ChipManager Instance;

    public GameObject[] Chips;
    public CanvasGroup cg;

    private void Awake()
    {
        Instance = this;
        cg = gameObject.AddComponent<CanvasGroup>();
    }

    public static GameObject InstantiateChip(int index)
    {
        return Instantiate(Instance.Chips[index]);
    }

    public static float GetSelectedValue()
    {
        if(selected != null)
            return selected.value;

        return 0;
    }

    public static void EnableChips(bool enable)
    {
        Instance.cg.interactable = enable;
    }
}