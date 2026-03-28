using UnityEngine;

public class ChipManager : MonoBehaviour
{

    public static Chip selected = null;
    private static ChipManager Instance;

    // Assign your chip prefabs here in the Inspector, in the same order as
    // ChipStack.CHIP_VALUES: 1, 5, 10, 25, 50, 100, 1K, 5K, 10K, 25K, 50K, 100K, 500K, 1M
    public GameObject[] Chips;
    public CanvasGroup cg;

    // Lets ChipStack know how many prefabs are actually assigned,
    // so it can fall back gracefully when high-value prefabs are missing.
    public static int ChipPrefabCount => Instance != null ? Instance.Chips.Length : 0;

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
        if (selected != null)
            return selected.value;

        return 0;
    }

    public static void EnableChips(bool enable)
    {
        Instance.cg.interactable = enable;
    }
}