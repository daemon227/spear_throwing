using System.Collections;
using UnityEngine;

namespace DongA.SpearManager
{
    public class SpearFlightMirror : SpearFlightBehavior
    {
        [Header("Mirror Settings")]
        [SerializeField] private LayerMask _mirrorLayer; // Layer cho gương
        [SerializeField] private float _reflectCheckDistance = 0.5f; // Khoảng cách check phía trước
        [SerializeField] private float _reflectOffset = 0.05f; // Offset sau khi phản xạ để tránh stuck
        
        private Rigidbody2D _rb;
        private float _speed;
        private Vector2 _currentDirection;
        private bool _isFlying = false;
        private Coroutine _reflectionCoroutine;

        public override void Launch(Rigidbody2D rb, Vector2 direction, float speed)
        {
            _rb = rb;
            _speed = speed;
            _currentDirection = direction.normalized;
            _isFlying = true;

            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.velocity = _currentDirection * _speed;
            rb.angularVelocity = 0f;

            // Bắt đầu coroutine kiểm tra phản xạ
            if (_reflectionCoroutine != null)
            {
                StopCoroutine(_reflectionCoroutine);
            }
            _reflectionCoroutine = StartCoroutine(ReflectionCheckRoutine());
        }

        private IEnumerator ReflectionCheckRoutine()
        {
            while (_isFlying && _rb != null)
            {
                CheckAndReflect();
                yield return new WaitForFixedUpdate();
            }
        }

        private void CheckAndReflect()
        {
            if (_rb == null || !_isFlying) return;

            // Raycast phía trước để kiểm tra gương
            RaycastHit2D hit = Physics2D.Raycast(
                _rb.position, 
                _currentDirection, 
                _reflectCheckDistance, 
                _mirrorLayer
            );

            if (hit.collider != null)
            {
                // Tính hướng phản xạ: R = D - 2(D·N)N
                Vector2 normal = hit.normal;
                Vector2 reflectedDirection = Vector2.Reflect(_currentDirection, normal);
                
                // Cập nhật vị trí để tránh stuck trong gương
                Vector2 newPosition = hit.point + normal * _reflectOffset;
                _rb.position = newPosition;
                
                // Cập nhật hướng và velocity
                _currentDirection = reflectedDirection.normalized;
                _rb.velocity = _currentDirection * _speed;
                
                // Xoay giáo theo hướng mới
                float angle = Mathf.Atan2(_currentDirection.y, _currentDirection.x) * Mathf.Rad2Deg;
                _rb.rotation = angle;
                
                Debug.Log($"Mirror reflection! New direction: {_currentDirection}");
            }
        }

        public override void Tick(Component controller)
        {
            // Có thể dùng để update liên tục nếu cần
        }

        /// <summary>
        /// Dừng phản xạ (gọi khi giáo stuck vào vật thể khác)
        /// </summary>
        public void StopReflection()
        {
            _isFlying = false;
            if (_reflectionCoroutine != null)
            {
                StopCoroutine(_reflectionCoroutine);
                _reflectionCoroutine = null;
            }
        }

        private void OnDisable()
        {
            StopReflection();
        }
    }
}
