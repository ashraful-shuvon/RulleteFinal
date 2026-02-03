using UnityEngine;
using System.Collections;

public class EuropeanWheel : Wheel
{
    private readonly new byte[] numbers = new byte[] { 0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13, 36, 11, 30, 8, 23,
        10, 5, 24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26 };

    public int Payout = 35;

    void Start()
    {
        BetSpace.numLenght = Payout;
        resultCheckerObject = new GameObject[numbers.Length];
        for (int i = 0; i < numbers.Length; i++)
        {
            int id = numbers[i];

            resultCheckerObject[id] = new GameObject("resultchecker");
            resultCheckerObject[id].transform.SetParent(transform);
            resultCheckerObject[id].transform.localPosition = new Vector3(0, .215f, 0);
            resultCheckerObject[id].transform.RotateAround(transform.position, Vector3.up, 180 + i * 360 / numbers.Length);
            
            resultCheckerObject[id].name = id.ToString();
            BetSpace.numLenght = 36;
        }
    }

    public override void Spin()
    {
        print("Spin European");
        base.Spin();
        StartCoroutine(SetResult());
    }

    private IEnumerator SetResult()
    {
        yield return new WaitForSecondsRealtime(5);
        ball.FindNumber(Random.Range(0, 36), true);
    }
}
