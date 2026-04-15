using DongA.SpearLogic;
using UnityEngine;

namespace DongA.SpearManager
{
    /// <summary>
    /// Flight behavior mặc định: bay thẳng, gravity = 0.
    /// Xoay spear sao cho freeTip luôn hướng theo chiều bay.
    /// Phản xạ gương được xử lý bởi SpearController (auto-launch từ mirror).
    /// </summary>
    public class SpearFlightDefault : SpearFlightBehavior
    {
        [Header("Acceleration Settings")]
        [SerializeField] [Range(0f, 1f)] private float initialSpeedRatio = 0.2f;
        [SerializeField] private float accelerationDuration = 0.4f;
        [SerializeField] private int accelerationPower = 2; // 2 = x², 4 = x⁴

        private SpearController _controller;
        private Rigidbody2D _rb;
        private Vector2 _currentDirection;
        private bool _isFlying;
        private float _maxSpeed;
        private float _elapsedTime;

        public override void Initialize(Component controller)
        {
            base.Initialize(controller);
            _controller = controller as SpearController;
        }

        public override void Launch(Rigidbody2D rb, Vector2 direction, float speed)
        {
            _rb = rb;
            _currentDirection = direction.normalized;
            _isFlying = true;
            _maxSpeed = speed;
            _elapsedTime = 0f;

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.gravityScale = 0f;
            rb.velocity = _currentDirection * (_maxSpeed * initialSpeedRatio);
            rb.angularVelocity = 0f;

            AlignSpearToDirection(_currentDirection);
        }

        private void FixedUpdate()
        {
            if (!_isFlying || _rb == null)
                return;

            if (_rb.bodyType == RigidbodyType2D.Static)
            {
                _isFlying = false;
                return;
            }

            // Tính tốc độ theo curve t^power (ease-in)
            _elapsedTime += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(_elapsedTime / accelerationDuration);
            float curve = Mathf.Pow(t, accelerationPower);
            float currentSpeed = Mathf.Lerp(_maxSpeed * initialSpeedRatio, _maxSpeed, curve);
            _rb.velocity = _currentDirection * currentSpeed;

            AlignSpearToDirection(_currentDirection);
        }

        public override void Stop(Rigidbody2D rb)
        {
            _isFlying = false;
            base.Stop(rb);
        }

        private void AlignSpearToDirection(Vector2 direction)
        {
            if (_controller == null || _rb == null)
                return;

            Transform freeTip = _controller.FreeTip;
            Transform rotatedTip = _controller.RotatedTip;

            if (freeTip == null || rotatedTip == null)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                _rb.MoveRotation(angle);
                return;
            }

            Vector2 tipDir = ((Vector2)freeTip.position - (Vector2)rotatedTip.position).normalized;
            if (tipDir.sqrMagnitude < 0.0001f)
                return;

            float deltaAngle = Vector2.SignedAngle(tipDir, direction.normalized);
            float targetZ = _controller.transform.eulerAngles.z + deltaAngle;
            _rb.MoveRotation(targetZ);
        }
    }
}

