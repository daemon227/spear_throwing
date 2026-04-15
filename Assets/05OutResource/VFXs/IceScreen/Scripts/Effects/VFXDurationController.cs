using Sirenix.OdinInspector;
using UnityEngine;

namespace MergePuzzle
{
    public class VFXDurationController : MonoBehaviour
    {
        [SerializeField] private float _originDuration;
        [SerializeField] private ParticleSystem[] _durationVfxes;
        [SerializeField] private ParticleSystem[] _lifetimeVfxes;

        [Button]
        public void SetDuration(float duration)
        {
            float extraValue = duration - _originDuration;
            for (int i = 0; i < _durationVfxes.Length; i++)
            {
                var main = _durationVfxes[i].main;
                main.duration += extraValue;
            }

            for (int i = 0; i < _lifetimeVfxes.Length; i++)
            {
                var main = _lifetimeVfxes[i].main;
                var lifetime = main.startLifetime;
                lifetime.constant += extraValue;
                main.startLifetime = lifetime;
            }
            _originDuration = duration;
        }
    }
}
