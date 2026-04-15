using System;
using System.Collections;
using DG.Tweening;
using DongA.Events;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float _shakeDuration = 0.2f;
    [SerializeField] private float _shakeMagnitude = 0.3f;

    [Header("Kill Slow Mo Camera")]
    [SerializeField] private float _killCamMoveDuration = 0.3f;
    [SerializeField] private float _killCamZoomSize = 3f;
    [SerializeField] private Ease _killCamEaseIn = Ease.OutCubic;
    [SerializeField] private Ease _killCamEaseOut = Ease.InOutCubic;

    private Vector3 _initialPosition;
    private float _initialOrthoSize;
    private Camera _cam;
    // private bool isLastEnemy;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _initialPosition = transform.localPosition;
        if (_cam != null) _initialOrthoSize = _cam.orthographicSize;

        GameEvents.OnSpearStuck += CameraShake;
        GameEvents.OnMultiSliceEffectCompleted += OnStrongShake;
        // GameEvents.OnKillSlowMoStart += OnKillSlowMoStart;
        // GameEvents.OnLevelStartLoading += ResetCamera;
    }

    void OnDestroy()
    {
        GameEvents.OnSpearStuck -= CameraShake;
        GameEvents.OnMultiSliceEffectCompleted -= OnStrongShake;
        // GameEvents.OnKillSlowMoStart -= OnKillSlowMoStart;
        // GameEvents.OnLevelStartLoading -= ResetCamera;
    }

    private void CameraShake()
    {
        // if(isLastEnemy) return;
        transform.DOShakePosition(_shakeDuration, _shakeMagnitude, 20, 90f, false, true)
                 .OnComplete(() => transform.localPosition = _initialPosition);
    }

    private void OnStrongShake()
    {
        // if(isLastEnemy) return;
        transform.DOShakePosition(_shakeDuration * 3f, _shakeMagnitude * 10f, 50, 90f, false, true)
                 .OnComplete(() => transform.localPosition = _initialPosition);
    }

    private void OnKillSlowMoStart(Vector3 targetWorldPos)
    {
        // isLastEnemy = true;
        transform.DOKill();
        if (_cam != null) _cam.DOKill();

        Vector3 camTarget = new Vector3(targetWorldPos.x, targetWorldPos.y, transform.position.z);
        transform.DOMove(camTarget, _killCamMoveDuration)
                 .SetEase(_killCamEaseIn)
                 .SetUpdate(true);

        if (_cam != null && _cam.orthographic)
        {
            _cam.DOOrthoSize(_killCamZoomSize, _killCamMoveDuration)
                .SetEase(_killCamEaseIn)
                .SetUpdate(true);
        }
    }

    private void ResetCamera()
    {
        // isLastEnemy = false;
        transform.DOKill();
        if (_cam != null) _cam.DOKill();

        transform.localPosition = _initialPosition;
        if (_cam != null && _cam.orthographic)
            _cam.orthographicSize = _initialOrthoSize;
    }
}
