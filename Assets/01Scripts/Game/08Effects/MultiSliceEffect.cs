using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DongA.Events;
using GameBase;

namespace DongA.Effects
{
    /// <summary>
    /// Object có Trail bay kiểu chém ngang Sukuna:
    ///   Bottom corner → zigzag lên đỉnh (CatmullRom curve) → đợi → lao xuống tâm đáy.
    ///   Child head sprite (mặc định hướng xuống) tự xoay theo hướng di chuyển.
    /// </summary>
    public class MultiSliceEffect : MonoBehaviour
    {
        [Header("Path (Viewport 0-1)")]
        [SerializeField] private float _xMargin    = 0.08f;
        [SerializeField] private float _startY     = 0.05f;
        [SerializeField] private float _topY       = 0.90f;
        [SerializeField] private float _minYStep   = 0.05f;
        [SerializeField] private float _maxYStep   = 0.15f;

        [Header("Timing (s)")]
        [SerializeField] private float _slashDuration  = 0.10f;
        [SerializeField] private float _waitAtTop      = 0.50f;
        [SerializeField] private float _dropDuration   = 0.28f;

        [Header("Head Rotation")]
        [SerializeField] private Transform _head;
        [SerializeField] private float _rotationSpeed = 20f;

        [Header("Ease")]
        [SerializeField] private Ease _slashEase   = Ease.Linear;
        [SerializeField] private Ease _dropEase    = Ease.InExpo;

        [Header("Sound")]
        [SerializeField] private AudioClip drawLineSfx;
        [SerializeField] private AudioClip killSfx;

        private Sequence _seq;
        private Camera   _cam;
        private Vector3  _prevPos;

        private void Awake()  => _cam = Camera.main;
        private void OnEnable()
        {
            Play();
            GameEvents.OnGameFinish += OnGameFinish;
        }
        private void OnDisable()
        {
            Kill();
            GameEvents.OnGameFinish -= OnGameFinish;
        }

        // ------------------------------------------------------------------ //

        public void Play()
        {
            Kill();

            BuildPath(out var zigzagPoints, out var dropTarget);

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySoundAtPoint(drawLineSfx, _cam.transform.position, 1f);

            // Teleport đến điểm đầu
            SetPos(zigzagPoints[0]);
            _prevPos = zigzagPoints[0];

            // Xoay head theo hướng di chuyển ban đầu
            if (_head != null && zigzagPoints.Count > 1)
            {
                Vector3 initDir = zigzagPoints[1] - zigzagPoints[0];
                float initAngle = Mathf.Atan2(initDir.y, initDir.x) * Mathf.Rad2Deg + 90f;
                _head.rotation = Quaternion.Euler(0, 0, initAngle);
            }

            // Build waypoints cho DOPath (bỏ điểm đầu vì đã teleport)
            var waypoints = new Vector3[zigzagPoints.Count - 1];
            for (int i = 1; i < zigzagPoints.Count; i++)
                waypoints[i - 1] = zigzagPoints[i];

            float zigzagDuration = _slashDuration * waypoints.Length;

            _seq = DOTween.Sequence().SetUpdate(true);

            // Zigzag CatmullRom — đường cong mềm mại qua tất cả waypoint
            _seq.Append(
                transform.DOPath(waypoints, zigzagDuration, PathType.CatmullRom)
                    .SetEase(_slashEase)
                    .OnUpdate(RotateHead)
            );

            // Đợi ở đỉnh
            _seq.AppendInterval(_waitAtTop);

            // Báo vị trí top rồi lao xuống
            _seq.AppendCallback(() => GameEvents.OnMultiSliceEffectDrop?.Invoke(transform.position));
            _seq.Append(
                transform.DOMove(dropTarget, _dropDuration)
                    .SetEase(_dropEase)
                    .OnUpdate(RotateHead)
            );

            _seq.Play();
            _seq.OnComplete(() =>
            {
                GameEvents.OnMultiSliceEffectCompleted?.Invoke();
                GamePrefs.IsUIBlocked = false;
                if (SoundManager.Instance != null)
                {
                    if(GamePrefs.SoundEnable) SoundManager.Instance.PlaySoundAtPoint(killSfx, _cam.transform.position, 1f);
                    if(GamePrefs.MusicEnable) SoundManager.Instance.PlayBGMSound(true);
                }
                Destroy(gameObject,0.5f);
            });
            
        }

        private void OnGameFinish(bool isWin)
        {
            Kill();
            GamePrefs.IsUIBlocked = false;
            Destroy(gameObject);
        }

        public void Kill()
        {
            _seq?.Kill();
            _seq = null;
        }

        // ------------------------------------------------------------------ //

        /// <summary>
        /// Xoay _head theo hướng di chuyển.
        /// Sprite mặc định hướng xuống (-Y) nên offset +90°.
        /// </summary>
        private void RotateHead()
        {
            if (_head == null) return;

            Vector3 dir = transform.position - _prevPos;
            if (dir.sqrMagnitude > 0.0001f)
            {
                float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
                Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle);
                _head.rotation = Quaternion.Lerp(_head.rotation, targetRot,
                    Time.unscaledDeltaTime * _rotationSpeed);
            }
            _prevPos = transform.position;
        }

        // ------------------------------------------------------------------ //

        private void BuildPath(out List<Vector3> zigzagPoints, out Vector3 dropTarget)
        {
            zigzagPoints = new List<Vector3>();

            // Điểm xuất phát: ngẫu nhiên trái hoặc phải
            bool startLeft = Random.value > 0.5f;
            float prevX = startLeft ? _xMargin : 1f - _xMargin;
            zigzagPoints.Add(VP(prevX, _startY));

            float y = _startY + Random.Range(_minYStep, _maxYStep);

            // Bay zigzag lên — mỗi nhát ra biên trái/phải
            while (y < _topY - _maxYStep * 0.5f)
            {
                bool goToLeft = prevX > 0.5f;
                if (Random.value < 0.3f) goToLeft = !goToLeft;

                float edgeX = goToLeft ? _xMargin : 1f - _xMargin;
                float randomOffset = Random.Range(0f, 0.12f) * (goToLeft ? 1f : -1f);
                float x = Mathf.Clamp(edgeX + randomOffset, _xMargin, 1f - _xMargin);

                zigzagPoints.Add(VP(x, y));

                prevX = x;
                y += Random.Range(_minYStep, _maxYStep);
            }

            // Điểm cuối zigzag: center top
            zigzagPoints.Add(VP(0.5f, _topY));

            // Drop target: center bottom
            dropTarget = VP(0.5f, _startY);
        }

        private Vector3 VP(float vx, float vy)
        {
            float depth = Mathf.Abs(_cam.transform.position.z - transform.position.z);
            Vector3 w = _cam.ViewportToWorldPoint(new Vector3(vx, vy, depth));
            w.z = transform.position.z;
            return w;
        }

        private void SetPos(Vector3 pos) => transform.position = pos;
    }
}
