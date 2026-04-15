using DongA.Events;
using UnityEngine;

namespace MergePuzzle
{
    public class IceScreenController : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _vfx;

        private void Awake()
        {
            Init();
        }
        void Start()
        {
            gameObject.SetActive(true);
            _vfx.Play();
        }

        private void Init()
        {
            float duration = 30f;
            _vfx.GetComponent<VFXDurationController>().SetDuration(duration);
        }
    }
}
