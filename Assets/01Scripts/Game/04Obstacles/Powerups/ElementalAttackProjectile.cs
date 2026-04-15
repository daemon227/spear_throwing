using DongA.Characters;
using UnityEngine;

namespace DongA.Powerups
{
    public class DeadByElementalAttack : MonoBehaviour
    {
        [SerializeField] private float speed = 15f;
        [SerializeField] private float maxLifetime = 3f;

        private CharacterBase _target;
        private Vector3 _targetPosition;
        private bool _initialized;
        private float _elapsedTime;
        private ElementalAttackType _elementType;

        public void Initialize(CharacterBase target, ElementalAttackType elementType)
        {
            _target = target;
            _targetPosition = target.transform.position;
            _elementType = elementType;
            _initialized = true;
            _elapsedTime = 0f;
        }

        private void Update()
        {
            if (!_initialized) return;

            _elapsedTime += Time.deltaTime;
            if (_elapsedTime >= maxLifetime)
            {
                Destroy(gameObject);
                return;
            }
            if (_target != null)
            {
                var collider = _target.GetComponentInChildren<Collider2D>();
                _targetPosition = collider != null ? (Vector3)collider.bounds.center : _target.transform.position;
            }

            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _targetPosition) < 0.1f)
            {
                if (_target != null)
                {
                    _target.DeadByElementalAttack(_elementType);
                }
                Destroy(gameObject);
            }
        }
    }
}
