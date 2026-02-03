using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    public Transform target;
    private Vector3 originPosition;
    private Quaternion originRotation;

    public float animationTime = 1;
    public Ease animationStyle = Ease.InOutSine;

    private void Start()
    {
        originPosition = transform.position;
        originRotation = transform.rotation;
    }

    public void GoToTarget()
    {
        if (target == null)
            return;

        Sequence sq = DOTween.Sequence();
        sq
            .Append(transform.DOMove(target.position, animationTime))
            .Join(transform.DORotate(target.rotation.eulerAngles, animationTime));
        sq.SetEase(animationStyle);
    }

    public void GoToOrigin()
    {
        if (target == null)
            return;
        Sequence sq = DOTween.Sequence();
        sq
            .Append(transform.DOMove(originPosition, animationTime))
            .Join(transform.DORotate(originRotation.eulerAngles, animationTime)).SetDelay(.45f);
        sq.SetEase(animationStyle);
    }
}
