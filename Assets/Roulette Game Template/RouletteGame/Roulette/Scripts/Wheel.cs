using UnityEngine;

public class Wheel : MonoBehaviour
{
    protected byte[] numbers;
    protected bool spinning = true;

    public BallManager ball;
    public GameObject[] resultCheckerObject;

    private readonly Vector3 pivot = Vector3.back;
    public readonly float speed = 0.3f;

    public static float lastBet = 0;

    void FixedUpdate()
    {
        if (spinning)
            transform.Rotate(pivot * speed);
    }

    public virtual void Spin()
    {
        BetSpace.EnableBets(false);
        SceneRoulette.GameStarted = true;
        ball.StartSpin();
    }
}
