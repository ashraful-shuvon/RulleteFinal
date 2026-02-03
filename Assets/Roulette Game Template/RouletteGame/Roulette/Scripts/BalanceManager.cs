using UnityEngine;

public class BalanceManager : MonoBehaviour {

    public static float Balance { get; private set; } = 0;

    public static void SetBalance(float balance)
    {
        Balance = balance;
        SceneRoulette.UpdateLocalPlayerText();
    }

    public static void ChangeBalance(float value)
    {
        Balance += value;
        SceneRoulette.UpdateLocalPlayerText();
    }

    public void ResetBalance(float balance)
    {
        Balance = balance;
        SceneRoulette.UpdateLocalPlayerText();
    }
}
