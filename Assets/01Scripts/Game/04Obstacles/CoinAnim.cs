using System.Collections.Generic;
using DG.Tweening;
using DongA.UI;
using UnityEngine;

namespace DongA.Obstacles
{
    public class CoinAnim : MonoBehaviour
    {
        private Canvas _canvas;
        
        [SerializeField] private Transform target;
        [SerializeField] private RectTransform coinObject;
        [SerializeField] private float moveDuration = 0.5f;
        [SerializeField] private float arcHeight = 2f;
        [SerializeField] private float spawnInterval = 0.12f;
        [SerializeField] private List<GameObject> coinPool = new List<GameObject>();

        private int _coinsPerUnit = 1;

        void Settup()
        {
            _canvas = GetComponent<Canvas>();
            if (_canvas != null)
            {
                _canvas.worldCamera = Camera.main;
            }
            if (UIManager.Instance != null)
                target = UIManager.Instance.coinBar != null ? UIManager.Instance.coinBar.transform : null;
        }

        private System.Action _onAllCoinsComplete;
        private int _coinsCompleted;

        /// <summary>
        /// Phát coin animation từ một vị trí world-space đến target tuỳ chỉnh.
        /// totalCoins được chia đều cho mỗi coin trong pool.
        /// onAllComplete được gọi sau khi coin cuối cùng chạm target.
        /// </summary>
        public void PlayFromWorldPos(Vector3 startWorldPos, Transform customTarget, int totalCoins = 1, System.Action onAllComplete = null)
        {
            target = customTarget;
            _coinsPerUnit = Mathf.Max(1, Mathf.RoundToInt((float)totalCoins / Mathf.Max(1, coinPool.Count)));
            _onAllCoinsComplete = onAllComplete;
            _coinsCompleted = 0;
            foreach (var coin in coinPool)
                if (coin != null) coin.transform.position = startWorldPos;
            ReceiveCoinAnim();
        }


        // Chuyển world position của coin → vị trí đúng trong Canvas Screen Space Camera
        public void PlaceCoinCollectAtWorldPos(Vector3 worldPos, int totalCoins = 1)
        {
            Settup();
            if (coinObject == null || _canvas == null) return;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.GetComponent<RectTransform>(),
                screenPos,
                _canvas.worldCamera,
                out Vector2 localPos
            );
            coinObject.anchoredPosition = localPos;
            _coinsPerUnit = Mathf.Max(1, Mathf.RoundToInt((float)totalCoins / Mathf.Max(1, coinPool.Count)));
            ReceiveCoinAnim();
        }

        public void ReceiveCoinAnim()
        {   
            if (coinPool.Count == 0 || target == null) return;

            const float spawnDuration = 0.22f;
            const float holdDuration  = 0.18f;
            const float dropDuration  = 0.13f;
            const float fadeDuration  = 0.15f;

            for (int i = 0; i < coinPool.Count; i++)
            {
                var coin = coinPool[i];
                if (coin == null) continue;

                var sr = coin.GetComponent<SpriteRenderer>();
                var initScale  = coin.transform.localScale;

                // Reset trạng thái cho pool reuse
                if (sr != null) sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
                coin.transform.localScale = Vector3.zero;
                coin.SetActive(true);

                float startDelay = i * spawnInterval;

                Sequence seq = DOTween.Sequence().SetTarget(coin).SetUpdate(true);
                seq.AppendInterval(startDelay);

                // Spawn: scale bật lên
                seq.Append(coin.transform.DOScale(initScale, spawnDuration).SetEase(Ease.OutBack));
                seq.AppendInterval(holdDuration);

                // Drop xuống nhẹ
                Vector3 coinPos = coin.transform.position;
                Vector3 dropPos = coinPos + new Vector3(Random.Range(-0.12f, 0.12f), -0.35f, 0f);
                Vector3 arcMid  = Vector3.Lerp(dropPos, target.position, 0.35f) + Vector3.up * arcHeight;

                seq.Append(coin.transform.DOMove(dropPos, dropDuration).SetEase(Ease.OutQuad));
                seq.Join(coin.transform.DOScale(initScale * 0.75f, dropDuration).SetEase(Ease.OutQuad));

                // Bay theo cung đến target
                seq.Append(coin.transform
                    .DOPath(new[] { arcMid, target.position }, moveDuration, PathType.CatmullRom)
                    .SetEase(Ease.InSine));
                seq.Join(coin.transform.DOScale(initScale * 1.15f, moveDuration * 0.25f).SetEase(Ease.OutBack));

                // Fade out cuối hành trình
                float fadeAt = startDelay + spawnDuration + holdDuration + dropDuration + moveDuration - fadeDuration;
                if (sr != null)
                    seq.Insert(fadeAt, sr.DOFade(0f, fadeDuration).SetEase(Ease.InQuad));

                // Cleanup — capture biến cho closure
                var c  = coin;
                var s  = sr;
                var sc = initScale;
                int coins = _coinsPerUnit;
                seq.OnComplete(() =>
                {
                    if (s != null) s.color = new Color(s.color.r, s.color.g, s.color.b, 1f);
                    c.transform.localScale = sc;
                    UIManager.Instance.RecieveCoinAnim(coins);
                    c.SetActive(false);
                    _coinsCompleted++;
                    if (_coinsCompleted >= coinPool.Count)
                    {
                        _onAllCoinsComplete?.Invoke();
                        _onAllCoinsComplete = null;
                    }
                });
            }
            
        }

    }
}


