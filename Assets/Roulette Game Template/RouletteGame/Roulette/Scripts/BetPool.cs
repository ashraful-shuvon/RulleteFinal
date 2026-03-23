using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BetPool : MonoBehaviour
{
    public static BetPool Instance;

    private Stack<BetFootprint> _BetFootprints;
    private List<BetSpace> _BetsList;
    private List<BetSpace> _RebetList;

    private void Awake()
    {
        if (Instance == null)
        {
            _BetFootprints = new Stack<BetFootprint>();
            _BetsList = new List<BetSpace>();

            Instance = this;
        }
        else
            Destroy(this.gameObject);
    }

    public void ResetStatus()
    {
        _BetFootprints.Clear();
        _RebetList = new List<BetSpace>(_BetsList);
        _BetsList.Clear();
    }
    
    public void Add(BetSpace space, float value)
    {
        _BetFootprints.Push(new BetFootprint(space, value));

        if (!_BetsList.Contains(space))
            _BetsList.Add(space);
    }

    public void Clear()
    {
        foreach (BetSpace bet in _BetsList)
            bet.Clear();

        _BetFootprints.Clear();
        _BetsList.Clear();

        ResultManager.totalBet = 0;
    }

    public void Undo()
    {
        BetFootprint footprint =  _BetFootprints.Pop();
        footprint.betSpace.RemoveBet(footprint.value);

        if(footprint.betSpace.GetValue() >= 0)
        {
            _BetsList.Remove(footprint.betSpace);
        }
    }

    public IEnumerator Rebet()
    {
        ResultManager.totalBet = 0;
        AudioManager.SoundPlay(3);

        foreach (BetSpace bet in _RebetList)
        {
            bet.Rebet();
            yield return null;
        }
    }
}

[System.Serializable]
public class BetFootprint
{
    public BetSpace betSpace;
    public float value;

    public BetFootprint(BetSpace betSpace, float value)
    {
        this.betSpace = betSpace;
        this.value = value;
    }
}

