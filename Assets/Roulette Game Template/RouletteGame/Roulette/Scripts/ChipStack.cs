using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System;

public class ChipStack : MonoBehaviour
{

    // Extended chip denominations — index must match ChipManager.Chips[] array order
    public static readonly int[] CHIP_VALUES = new int[] { 1, 5, 10, 25, 50, 100, 1000, 5000, 10000, 25000, 50000, 100000, 500000, 1000000 };

    // Maximum physical chips rendered in the scene to avoid performance issues
    private const int MAX_VISIBLE_CHIPS = 20;

    public static readonly Vector3 CollectPosition = new Vector3(0, 0, -3);

    private Vector3 initialPosition;
    private float value = 0;

    private List<GameObject> chips;

    void Start()
    {
        initialPosition = transform.position;
    }

    public void SetInitialPosition(Vector3 pos)
    {
        transform.position = pos;
        initialPosition = pos;
    }

    public void Add(float value)
    {
        SetValue(this.value + value);
    }

    public void Remove(float value)
    {
        SetValue(this.value - value);
    }

    public float Clear()
    {
        float lastBet = value;
        value = 0;
        transform.position = initialPosition;

        if (chips != null)
        {
            foreach (GameObject chip in chips)
            {
                Destroy(chip);
            }
        }
        chips = null;
        return lastBet;
    }

    public float GetValue()
    {
        return value;
    }

    public void SetValue(float value)
    {
        Clear();

        if (value <= 0)
        {
            return;
        }

        this.value = value;
        chips = new List<GameObject>();

        // --- Greedy decomposition: find the minimum set of chip denominations ---
        float remaining = value;
        int currentChipIndex = CHIP_VALUES.Length - 1;

        // Build a list of (chipIndex, count) pairs
        List<(int index, int count)> chipGroups = new List<(int, int)>();

        while (remaining > 0.001f)
        {
            if (currentChipIndex < 0)
                throw new Exception("Impossible chip value: " + value);

            int chipVal = CHIP_VALUES[currentChipIndex];
            int count = (int)(remaining / chipVal);

            if (count > 0)
            {
                chipGroups.Add((currentChipIndex, count));
                remaining -= chipVal * count;
                remaining = Mathf.Round(remaining * 100f) / 100f; // fix float drift
            }

            currentChipIndex--;
        }

        // --- Render chips, capped at MAX_VISIBLE_CHIPS to avoid spawning thousands ---
        int totalLogicalChips = 0;
        foreach (var g in chipGroups) totalLogicalChips += g.count;

        int slotIndex = 0;

        if (totalLogicalChips <= MAX_VISIBLE_CHIPS)
        {
            // Render every chip normally
            foreach (var (index, count) in chipGroups)
            {
                for (int i = 0; i < count; i++)
                {
                    SpawnChip(index, slotIndex++);
                }
            }
        }
        else
        {
            // Show one representative chip per denomination group only
            foreach (var (index, count) in chipGroups)
            {
                if (count > 0)
                    SpawnChip(index, slotIndex++);
            }
        }
    }

    private void SpawnChip(int chipIndex, int stackSlot)
    {
        // Clamp index so missing high-value prefabs fall back to the highest available
        int safeIndex = Mathf.Clamp(chipIndex, 0, ChipManager.ChipPrefabCount - 1);

        GameObject newChip = ChipManager.InstantiateChip(safeIndex);
        newChip.transform.parent = gameObject.transform;
        newChip.transform.localPosition = new Vector3(0, .01f * (stackSlot + 1), 0);
        chips.Add(newChip);
    }

    public float Win(int multiplier)
    {
        float profit = value * multiplier;
        SetValue(profit);

        if (profit > 0)
        {
            CollectChips();
        }
        return profit;
    }

    public void CollectChips()
    {
        transform.DOMove(CollectPosition, 1).SetEase(Ease.InSine).SetDelay(1.5f).OnComplete(() => { Clear(); });
    }
}