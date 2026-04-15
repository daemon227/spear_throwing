using DG.Tweening;
using GameBase;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DongA.UI
{
    /// <summary>
    /// Attach ONE instance to a Canvas root.
    /// Automatically adds ButtonSoundTrigger to every Button found in children.
    /// Call InjectAll() manually after spawning dynamic UI if needed.
    /// </summary>
    public class ButtonSoundInjector : MonoBehaviour
    {
        private void Start()
        {
            InjectAll();
        }

        public void InjectAll()
        {
            var buttons = GetComponentsInChildren<Button>(includeInactive: true);
            foreach (var btn in buttons)
                Inject(btn);
        }

        public static void Inject(Button btn)
        {
            if (btn.GetComponent<ButtonSoundTrigger>() == null)
                btn.gameObject.AddComponent<ButtonSoundTrigger>();
        }
    }

    /// <summary>
    /// Plays sound on pointer DOWN with bounce animation.
    /// Blocks clicks while the animation is playing.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonSoundTrigger : MonoBehaviour, IPointerClickHandler
    {
        private bool _isAnimating;
        private Vector3 _originalScale;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isAnimating) return;

            if (SoundManager.Instance != null) SoundManager.Instance.PlayUISound();

            _isAnimating = true;

            transform.DOKill();
            transform.localScale = _originalScale;
            transform.DOPunchScale(_originalScale * 0.15f, 0.3f, 8, 0.5f)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    transform.localScale = _originalScale;
                    _isAnimating = false;
                });
        }
    }
}
