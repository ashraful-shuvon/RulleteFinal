using System.Collections.Generic;
using UnityEngine;

public class GhostStack : MonoBehaviour
{
    private Vector3 initialPosition;
    private float value = 0;

    private List<GameObject> chips;

    private readonly Color ghostColor = new Color(1,1,1,.7f);

    public void SetInitialPosition(Vector3 pos)
    {
        transform.position = pos;
        initialPosition = pos;
    }

    public void Clear()
    {
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

        int currentChipIndex = ChipStack.CHIP_VALUES.Length - 1;

        while (value > 0)
        {
            float nextValue = value - ChipStack.CHIP_VALUES[currentChipIndex];

            if (nextValue < 0)
            {
                currentChipIndex--;
                if (currentChipIndex < 0)
                {
                    throw new System.Exception("Impossible value");
                }
                continue;
            }

            value = nextValue;

            GameObject newChip = ChipManager.InstantiateChip(currentChipIndex);
            newChip.GetComponent<SpriteRenderer>().color = ghostColor;
            newChip.transform.parent = gameObject.transform;
            newChip.transform.localPosition = new Vector3(0, .01f * (chips.Count + 1), 0);

            chips.Add(newChip);
        }
    }
}
