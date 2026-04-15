using DG.Tweening;
using UnityEngine;

public class MoveAnchorPos : MonoBehaviour
{
    [SerializeField] private Vector2 _anchorPos;
    [SerializeField] private float _duration = 1f;
    [SerializeField] private float _delay = 0f;
    [SerializeField] private Ease _ease = Ease.OutQuad;

    private void Start()
    {
        GetComponent<RectTransform>().DOAnchorPos(_anchorPos, _duration).SetDelay(_delay).SetEase(_ease);
    }
}
