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
        int spaceIndex = BetSpaceRegistry.GetIndexOfBetSpace(this);
        
        if (NetworkPlayer.LocalPlayer == null)
        {
            Debug.LogWarning("NetworkPlayer not found. Betting disabled.");
            return;
        }

        // The limits and balance checks are now handled locally & server-side
        // But we still apply local checks for feedback if needed.
        if (NetworkPlayer.LocalPlayer.PlaceBetOnSpace(spaceIndex, selectedValue))
        {
            AudioManager.SoundPlay(3);
            stack.Add(selectedValue);
            lastBet = stack.GetValue();
            
            // Replaced the old manual UI interation code here with the automated GameHUD updates.
            SceneRoulette.UpdateLocalPlayerText();
        }
    }

    public void RemoveBet(float value)
    {
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
                return 3;           // ← 2:1 for Dozen and Column

            case BetType.Red:
            case BetType.Black:
            case BetType.Even:
            case BetType.Odd:
            case BetType.Low:
            case BetType.High:
                return 2;

            default:
                return numLenght / numCount;
        }
    }
    public void Rebet()
    {
        // Handled completely by NetworkPlayer now. 
        // This visual space doesn't need to push rebet requests directly like the old system.
    }
    
    public void Clear()
    {
        lastBet = 0;
        stack.Clear();
        SceneRoulette.UpdateLocalPlayerText();
    }

    public static void EnableBets(bool enable)
    {
        BetsEnabled = enable;
    }

}