using UnityEngine;

[System.Serializable]
public enum BetType
{
    Straight,
    Split,
    Corner,
    Street,
    DoubleStreet,
    Row,
    Dozen,
    Low,
    High,
    Even,
    Odd,
    Red,
    Black
}

public class BetSpace : MonoBehaviour {

    public ChipStack stack;
    public BetType betType;
    public static int numLenght = 37; //Change this to change the amount of rewards

    [SerializeField]
    public int[] winningNumbers;

    public MeshRenderer[] betSpaceRender;

    private MeshRenderer mesh;
    private float lastBet = 0;

    public static bool BetsEnabled { get; private set; } = true;

    public float GetValue() => stack.GetValue();

    void Start()
    {
        mesh = GetComponent<MeshRenderer>();

        if (mesh)
            mesh.enabled = false;

        stack = Cloth.InstanceStack();
        stack.SetInitialPosition(transform.position);
        stack.transform.SetParent(transform);
        stack.transform.localPosition = Vector3.zero;
        ResultManager.RegisterBetSpace(this);
        //AmericanWheel.OnRebetAndSpin += Rebet;
    }

    private void OnMouseEnter()
    {
        ToolTipManager.SelectTarget(stack);

        if (mesh)
            mesh.enabled = true;

        if (!BetsEnabled)
            return;


        if (betSpaceRender.Length > 0)
        {
            foreach (MeshRenderer spaceRender in betSpaceRender)
            {
                spaceRender.enabled = true;
            }
        }
    }

    void OnMouseExit()
    {
        ToolTipManager.Deselect();

        if (mesh)
            mesh.enabled = false;

        if (!BetsEnabled)
            return;

        if (betSpaceRender.Length > 0)
        {
            foreach (MeshRenderer spaceRender in betSpaceRender)
            {
                spaceRender.enabled = false;
            }
        }
    }

    private void OnMouseUp()
    {
        float selectedValue = ChipManager.GetSelectedValue();
        ApplyBet(selectedValue);
        ToolTipManager.SelectTarget(stack);
    }

    public void ApplyBet(float selectedValue)
    {
        if (!LimitBetPlate.AllowLimit(selectedValue))
            return;

        if (BetsEnabled && selectedValue > 0 && BalanceManager.Balance - selectedValue >= 0)
        {
            AudioManager.SoundPlay(3);
            print("Bet applyed! with: " + selectedValue );

            BalanceManager.ChangeBalance(-selectedValue);
            ResultManager.totalBet += selectedValue;
            stack.Add(selectedValue);

            lastBet = stack.GetValue();

            BetPool.Instance.Add(this, selectedValue);

            SceneRoulette._Instance.clearButton.interactable = true;
            SceneRoulette._Instance.undoButton.interactable = true;
            SceneRoulette._Instance.rollButton.interactable = true;
            SceneRoulette._Instance.rebetButton.gameObject.SetActive(false);
            SceneRoulette.UpdateLocalPlayerText();
        }
    }

    public void RemoveBet(float value)
    {
        BalanceManager.ChangeBalance(value);
        ResultManager.totalBet -= value;
        stack.Remove(value);
        lastBet = stack.GetValue();
        SceneRoulette.UpdateLocalPlayerText();
    }

    /*
    public float ResolveBet(int result)
    {
        int multiplier = numLenght / winningNumbers.Length;

        bool won = false;

        foreach (int num in winningNumbers)
        {
            if (num == result)
            {
                won = true;

                //if (mesh && betType == BetType.Straight)
                //    mesh.enabled = true;
                break;
            }
        }

        float winAmount = 0;

        if (won)
        {
            winAmount = stack.Win(multiplier);
        } else
        {
            stack.Clear();
        }

        return winAmount;
    }
    */

    public float ResolveBet(int result)
    {
        bool won = false;

        foreach (int num in winningNumbers)
        {
            if (num == result)
            {
                won = true;
                break;
            }
        }

        float winAmount = 0;

        if (won)
        {
            // Use the new function instead of the old formula
            int multiplier = GetPayoutMultiplier(betType, winningNumbers.Length);

            Debug.Log("Bet Type: " + betType + " | Multiplier: " + multiplier + " | Bet Value: " + stack.GetValue());

            winAmount = stack.Win(multiplier);
        }
        else
        {
            stack.Clear();
        }

        return winAmount;
    }

    // Add this new function
    private int GetPayoutMultiplier(BetType type, int numCount)
    {
        switch (type)
        {
            case BetType.Straight:
                return 35;

            case BetType.Split:
                return 17;

            case BetType.Street:
                return 11;

            case BetType.Corner:
                return 8;

            case BetType.DoubleStreet:
                return 5;

            case BetType.Dozen:
            case BetType.Row:
                return 2;           // ← 2:1 for Dozen and Column

            case BetType.Red:
            case BetType.Black:
            case BetType.Even:
            case BetType.Odd:
            case BetType.Low:
            case BetType.High:
                return 1;

            default:
                return numLenght / numCount;
        }
    }
    public void Rebet()
    {
        if (lastBet == 0)
            return;

        if (!LimitBetPlate.AllowLimit(lastBet))
        {
            lastBet = 0;
            return;
        }

        if (BetsEnabled && BalanceManager.Balance - lastBet >= 0)
        {
            BalanceManager.ChangeBalance(-lastBet);
            ResultManager.totalBet += lastBet;
            stack.SetValue(lastBet);
            lastBet = stack.GetValue();

            BetPool.Instance.Add(this, lastBet);

            SceneRoulette._Instance.clearButton.interactable = true;
            SceneRoulette._Instance.undoButton.interactable = true;
            SceneRoulette._Instance.rollButton.interactable = true;
            SceneRoulette._Instance.rebetButton.gameObject.SetActive(false);
            SceneRoulette.UpdateLocalPlayerText();
        }
        else
            lastBet = 0;
    }
    
    public void Clear()
    {
        float val = stack.GetValue();
        BalanceManager.ChangeBalance(val);
        ResultManager.totalBet -= val;
        lastBet = 0;

        stack.Clear();
        SceneRoulette.UpdateLocalPlayerText();
    }

    public static void EnableBets(bool enable)
    {
        BetsEnabled = enable;
    }

}