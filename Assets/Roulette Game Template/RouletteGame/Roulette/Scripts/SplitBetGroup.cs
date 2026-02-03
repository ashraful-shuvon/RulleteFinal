using UnityEngine;
using System.Collections;

public class SplitBetGroup : MonoBehaviour {

    public BetSpace[] spaces;

    public int number = 1;

	void Start () {
        // Horizontal splits
        spaces[0].winningNumbers = new int[] { number + 2, number + 5 };
        spaces[1].winningNumbers = new int[] { number + 1, number + 4 };
        spaces[2].winningNumbers = new int[] { number, number + 3 };

        // Verticle splits
        spaces[3].winningNumbers = new int[] { number + 5, number + 4 };
        spaces[4].winningNumbers = new int[] { number + 4, number + 3 };

        // Corners
        if (spaces.Length > 5)
        {
            spaces[5].winningNumbers = new int[] { number + 4, number + 5, number + 7, number + 8 };
            spaces[6].winningNumbers = new int[] { number + 3, number + 4, number + 6, number + 7 };
        }
    }
}
