using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace DongA.UI
{
    public class SpearControlTutorial : MonoBehaviour
    {
        [Header("Ref")]
        [SerializeField] private Image handIcon;
        [SerializeField] private Sprite handUpImage;
        [SerializeField] private Sprite handDownImage;
        [SerializeField] private float moveDistance = 5f;
        [SerializeField] private float moveDuration = 2f;
        [SerializeField] private float delayTime = 0.5f;
        [SerializeField] private SkeletonGraphic spearSkeleton;

        private Vector3 _startPos;
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
            _startPos = handIcon.transform.localPosition;
            PlaySequence();
        }

        private void OnDisable()
        {
            _sequence?.Kill();
            handIcon.transform.localPosition = _startPos;
            handIcon.sprite = handUpImage;
            spearSkeleton.AnimationState.SetAnimation(0, ANIM_IDLE2, true);
        }

        private void PlaySequence()
        {
            handIcon.transform.localPosition = _startPos;
            handIcon.sprite = handUpImage;
            spearSkeleton.AnimationState.SetAnimation(0, ANIM_IDLE2, true);

            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            // Hiện handUp rồi chờ 0.5s
            _sequence.AppendInterval(0.5f);

            // Chuyển sang handDown
            _sequence.AppendCallback(() => handIcon.sprite = handDownImage);
            _sequence.AppendInterval(0.2f);

            // Di chuyển xuống + chạy anim IDle
            _sequence.AppendCallback(() => spearSkeleton.AnimationState.SetAnimation(0, ANIM_IDLE, true));
            _sequence.Append(handIcon.transform
                .DOLocalMoveY(_startPos.y - moveDistance, moveDuration)
                .SetEase(Ease.Linear));

            // Chờ rồi lặp lại
            _sequence.AppendInterval(delayTime);
            _sequence.OnComplete(PlaySequence);
        }
    }
}
