using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class MoveToTarget : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _duration = 1f;
    [SerializeField] private float _delay = 0f;
    [SerializeField] private Ease _ease = Ease.OutQuad;
    [SerializeField] private UnityEvent _onCompleted;

    public float Duration => _duration;
    public Transform Target { get => _target; set => _target = value; }

    private void Start()
    {
        transform.DOMove(_target.position, _duration).SetDelay(_delay).SetEase(_ease).OnComplete(() => _onCompleted?.Invoke());
    }
}
