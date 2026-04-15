using UnityEngine;
using DG.Tweening;
using DongA.Events;

namespace DongA.Effects
{
    public class BlackHoleEffect : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private GameObject _blackground;
        private void OnEnable()
        {
            _canvas.worldCamera = Camera.main;
            GameEvents.OnMultiSliceEffectCompleted += Kill;
        }

        private void OnDisable()
        {
            GameEvents.OnMultiSliceEffectCompleted -= Kill;
        }
        void Kill()
        {
            _blackground.SetActive(false);
        }
    }
}
