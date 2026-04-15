using System.Collections;
using System.Collections.Generic;
using DongA.Characters;
using UnityEngine;

namespace DongA.Powerups
{
    /// <summary>
    /// A projectile that freezes enemies on contact.
    /// </summary>
    public class IceProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 15f;
        [SerializeField] private float maxLifetime = 3f;
        [SerializeField] private GameObject iceImage;

        private CharacterBase _target;
        private Vector3 _targetPosition;
        private bool _initialized;
        private float _elapsedTime;

        public void Initialize(CharacterBase target)
        {
            _target = target;
            _targetPosition = target.transform.position;
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
                    _target.Freeze();
                    if (iceImage != null)
                    {
                        var rootCollider = _target.GetComponentInChildren<Collider2D>();
                        Vector3 spawnPos = rootCollider != null ? rootCollider.bounds.center : _target.transform.position;

                        var ice = Instantiate(iceImage, spawnPos, Quaternion.identity);
                        ice.transform.localScale = CaluateScale(_target.transform.lossyScale) * ice.transform.localScale;
                        Debug.Log("target name: " + _target.name );
                        ice.transform.SetParent(_target.transform);
                    }
                }
                Destroy(gameObject);
            }
        }
        

        private Vector2 CaluateScale(Vector2 characterWorldScale)
        {
            float x = Mathf.Abs(characterWorldScale.x) / 0.15f;
            float y = Mathf.Abs(characterWorldScale.y) / 0.15f;
            return new Vector2(x, y);
        }
    }
}
