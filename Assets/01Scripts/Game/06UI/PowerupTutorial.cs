using DG.Tweening;
using Spine.Unity;
using UnityEngine;

namespace DongA.UI
{
    public class PowerupTutorial : MonoBehaviour
    {
        [SerializeField] private SkeletonGraphic _mainSpearUI;
        [SerializeField] private float _moveDistance = 400f;
        [SerializeField] private float _moveDuration = 0.5f;
        [SerializeField] private float _cloneFlyDistance = 200f;
        [SerializeField] private float _cloneFlyDuration = 0.6f;
        [SerializeField] private float _angleBetweenSpears = 20f;
        [SerializeField] private float _loopDelay = 0.3f;

        private Vector3 _startPos;
        private SkeletonGraphic[] _clones;
        private Sequence _sequence;
        private const string ANIM_IDLE = "IDle";
        private const string ANIM_IDLE2 = "IDle2";
        private Canvas canvas;
        void Start()
        {
            canvas = GetComponent<Canvas>();
            if(canvas != null)
            {
                canvas.worldCamera = Camera.main;
            }
        }

        private void OnEnable()
        {
            _startPos = _mainSpearUI.transform.localPosition;
            PlaySequence();
        }

        private void OnDisable()
        {
            _sequence?.Kill();
            DestroyClones();
            _mainSpearUI.transform.localPosition = _startPos;
            _mainSpearUI.gameObject.SetActive(true);
        }

        private void PlaySequence()
        {
            DestroyClones();
            _mainSpearUI.transform.localPosition = _startPos;
            _mainSpearUI.gameObject.SetActive(true);

            _mainSpearUI.AnimationState.SetAnimation(0, ANIM_IDLE2, true);

            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            // Phase 0: IDle2 → IDle → chờ 0.5s → IDle2 đồng thời bay
            _sequence.AppendCallback(() => _mainSpearUI.AnimationState.SetAnimation(0, ANIM_IDLE, false));
            _sequence.AppendInterval(0.5f);
            _sequence.AppendCallback(() => _mainSpearUI.AnimationState.SetAnimation(0, ANIM_IDLE2, true));

            // Phase 1: main spear bay lên _moveDistance (đồng thời với IDle2)
            _sequence.Append(_mainSpearUI.transform
                .DOLocalMoveY(_startPos.y + _moveDistance, _moveDuration)
                .SetEase(Ease.OutQuad));

            // Phase 2: spawn 2 clone, tất cả 3 bay tiếp _cloneFlyDuration
            _sequence.AppendCallback(SpawnAndFlyClones);
            _sequence.AppendInterval(_cloneFlyDuration);

            // Phase 3: ẩn tất cả rồi loop
            _sequence.AppendCallback(HideAll);
            _sequence.AppendInterval(_loopDelay);
            _sequence.OnComplete(PlaySequence);
        }

        private void SpawnAndFlyClones()
        {
            Vector3 arrivalPos = _mainSpearUI.transform.localPosition;
            float[] angles = { -_angleBetweenSpears, _angleBetweenSpears };
            _clones = new SkeletonGraphic[angles.Length];

            for (int i = 0; i < angles.Length; i++)
            {
                var clone = Instantiate(_mainSpearUI, _mainSpearUI.transform.parent);
                clone.transform.SetLocalPositionAndRotation(arrivalPos, Quaternion.Euler(0f, 0f, angles[i]));
                clone.gameObject.SetActive(true);
                _clones[i] = clone;

                Vector2 dir = RotateVector(Vector2.up, angles[i]);
                Vector3 targetPos = arrivalPos + (Vector3)(dir * _cloneFlyDistance);
                clone.transform.DOLocalMove(targetPos, _cloneFlyDuration).SetEase(Ease.Linear);
            }

            // Main spear tiếp tục bay thẳng lên
            _mainSpearUI.transform.DOLocalMoveY(arrivalPos.y + _cloneFlyDistance, _cloneFlyDuration)
                .SetEase(Ease.Linear);
        }

        private void HideAll()
        {
            _mainSpearUI.gameObject.SetActive(false);
            if (_clones == null) return;
            foreach (var clone in _clones)
                if (clone != null) clone.gameObject.SetActive(false);
        }

        private void DestroyClones()
        {
            if (_clones == null) return;
            foreach (var clone in _clones)
                if (clone != null) Destroy(clone.gameObject);
            _clones = null;
        }

        private static Vector2 RotateVector(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }
    }
}
