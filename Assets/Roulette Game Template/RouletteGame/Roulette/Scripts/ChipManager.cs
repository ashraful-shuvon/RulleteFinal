using UnityEngine;

public class ChipManager : MonoBehaviour {

    // Removed legacy Canvas dependencies
    public static float currentlySelectedValue = 10f; // Default 10 chip

    private static readonly int[] CHIP_VALUES = new int[] { 1, 5, 10, 25, 50, 100 };

    public static GameObject InstantiateChip(int index)
    {
        if (index < 0 || index >= CHIP_VALUES.Length) return null;

        GameObject prefab = Resources.Load<GameObject>($"Chips/chip{CHIP_VALUES[index]}");
        if (prefab != null)
        {
            return Instantiate(prefab);
        }
        else
        {
            Debug.LogError($"[ChipManager] Missing prefab at Resources/Chips/chip{CHIP_VALUES[index]}");
            return null;
        }
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