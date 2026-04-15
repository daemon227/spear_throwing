using System.Collections;
using DG.Tweening;
using Inwave.Production.Ads;
using UnityEngine;
using UnityEngine.UI;

namespace DongA.UI
{
    public class UIMRec : MonoBehaviour
    {
        [SerializeField] private MaxSdkBase.AdViewPosition _position;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _sun;
        [SerializeField] private GameObject _vfx;
        [SerializeField] private Image[] _dots;

        private const float DOT_STEP     = 0.28f;
        private const float DOT_HOLD     = 0.30f;
        private const float DOT_DIM      = 0.25f;
        private const float SUN_OFFSET_Y = 50f;

        private Coroutine _dotCoroutine;
        private bool _isShowing;

        public string Location ="";

        private void Awake()
        {
            AdsEvents.MRec.OnAdShowed += OnMRecAdShowedEvent;
        }

        private void Start()
        {
            AdsUtils.HideBanner();
            AdsUtils.ShowMRecAd(Location);
            if (!_isShowing) PlayEntrance();
        }

        private void OnDestroy()
        {
            AdsEvents.MRec.OnAdShowed -= OnMRecAdShowedEvent;
            AdsUtils.HideMRecAd();
            AdsUtils.ShowBanner();
            KillAll();
        }

        private void OnMRecAdShowedEvent()
        {
            _isShowing = true;
            _canvasGroup.DOKill();
            _canvasGroup.alpha = 0f;
            _vfx.SetActive(false);
            transform.DOKill();
            StopAllCoroutines();
        }

        // ── Entrance ────────────────────────────────────────────────────────────


        private void PlayEntrance()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.DOFade(1f, 0.35f).SetEase(Ease.OutQuad).SetUpdate(true);
            }

            transform.localScale = Vector3.one * 0.85f;
            transform.DOScale(1f, 0.4f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .OnComplete(PlayDotsAnimation);
        }

        // ── Dots wave animation ──────────────────────────────────────────────────

        private void PlayDotsAnimation()
        {
            if (_dots == null || _dots.Length == 0) return;

            foreach (var dot in _dots)
            {
                dot.color = new Color(dot.color.r, dot.color.g, dot.color.b, DOT_DIM);
                dot.transform.localScale = Vector3.one;
            }

            if (_sun != null)
            {
                var firstDotPos = ((RectTransform)_dots[0].transform).anchoredPosition;
                _sun.anchoredPosition = new Vector2(firstDotPos.x, firstDotPos.y + SUN_OFFSET_Y);
            }

            if (_dotCoroutine != null) StopCoroutine(_dotCoroutine);
            _dotCoroutine = StartCoroutine(DotsLoop());
        }

        private IEnumerator DotsLoop()
        {
            // Build yoyo index list: 0,1,...,n-1,n-2,...,1
            var sunAngleStep = 360f / _dots.Length;
            var sunDuration = DOT_STEP * 2 + DOT_HOLD;
            var indices = new System.Collections.Generic.List<int>();
            for (int i = 0; i < _dots.Length; i++) indices.Add(i);
            for (int i = _dots.Length - 2; i >= 1; i--) indices.Add(i);

            while (true)
            {
                var sunAngle = 0f;
                foreach (int i in indices)
                {
                    var dot = _dots[i];
                    var dotRect = (RectTransform)dot.transform;

                    // Move sun + bounce dot simultaneously
                    if (_sun != null)
                    {
                        var sunTarget = new Vector2(dotRect.anchoredPosition.x, dotRect.anchoredPosition.y + SUN_OFFSET_Y);
                        if (sunTarget.x > _sun.anchoredPosition.x) sunAngle -= sunAngleStep;
                        else sunAngle += sunAngleStep;
                        _sun.DOKill();
                        _sun.DOLocalJump(sunTarget, SUN_OFFSET_Y, 1, sunDuration).SetUpdate(true);
                        _sun.DOLocalRotate(new Vector3(0f, 0f, sunAngle), sunDuration).SetUpdate(true);
                    }

                    dot.DOFade(1f, DOT_STEP).SetEase(Ease.OutQuad).SetUpdate(true);
                    dot.transform.DOScale(1.25f, DOT_STEP).SetEase(Ease.OutBack).SetUpdate(true);

                    yield return new WaitForSecondsRealtime(DOT_STEP);
                    yield return new WaitForSecondsRealtime(DOT_HOLD);

                    dot.DOFade(DOT_DIM, DOT_STEP).SetEase(Ease.InQuad).SetUpdate(true);
                    dot.transform.DOScale(1f, DOT_STEP).SetEase(Ease.InQuad).SetUpdate(true);

                    yield return new WaitForSecondsRealtime(DOT_STEP);
                }
            }
        }

        // ── Cleanup ──────────────────────────────────────────────────────────────

        private void KillAll()
        {
            if (_dotCoroutine != null) StopCoroutine(_dotCoroutine);
            if (_sun != null) _sun.DOKill();

            if (_dots != null)
                foreach (var dot in _dots)
                    if (dot != null) dot.transform.DOKill();
        }
    }
}
