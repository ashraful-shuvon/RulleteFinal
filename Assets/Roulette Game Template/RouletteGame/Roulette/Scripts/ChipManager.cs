using UnityEngine;

public class ChipManager : MonoBehaviour {

    // Removed legacy Canvas dependencies
    public static float currentlySelectedValue = 10f; // Default 10 chip
    private static ChipManager Instance;

    public GameObject[] Chips; // 3D Chips for visual instantiation

    private void Awake()
    {
        Instance = this;
    }

    public static GameObject InstantiateChip(int index)
    {
        if (Instance == null || Instance.Chips == null || Instance.Chips.Length <= index) return null;
        return Instantiate(Instance.Chips[index]);
    }

    public static float GetSelectedValue()
    {
        return currentlySelectedValue;
    }

    public static void SelectChipValue(float val)
    {
        currentlySelectedValue = val;
    }

    public static void EnableChips(bool enable)
    {
        // UI Toolkit handles this now if needed
    }
}