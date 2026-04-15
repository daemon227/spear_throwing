using DG.Tweening;
using DongA.Characters;
using UnityEngine;

namespace DongA.Obstacles
{
    public enum VehicalMoveMode
    {
        Straight,
        Curve
    }

    public class Vehical : ObstacleBase
    {
        [Header("Vehical Settings")]
        [SerializeField] private VehicalMoveMode vehicalMoveMode = VehicalMoveMode.Straight;
        [SerializeField] private Vector3 pointB;
        [SerializeField] private float curveHeight = 2f;
        [SerializeField] private bool usePhysicsFall;
        [SerializeField] private int curveSegments = 20;

        [Header("Enemy Settings")]
        [SerializeField] private Transform enemyPos;

        public VehicalMoveMode VehicalMoveModeType { get => vehicalMoveMode; set => vehicalMoveMode = value; }
        public Vector3 PointB { get => pointB; set => pointB = value; }
        public float CurveHeight { get => curveHeight; set => curveHeight = value; }
        public bool UsePhysicsFall { get => usePhysicsFall; set => usePhysicsFall = value; }
        public Transform EnemyPos { get => enemyPos; set => enemyPos = value; }

        private Vector3 _pointA;
        private Tween _moveTween;
        private bool _isBroken;
        private bool _initialized;
        private GameObject _enemyObject;

        public Vehical(ObstaclesMoveType moveType, float moveDistance, float moveDuration, float waitTime)
            : base(moveType, moveDistance, moveDuration, waitTime)
        {
        }

        void Start()
        {
            Init();
        }

        public void Init()
        {
            if (_initialized) return;
            _initialized = true;
            _pointA = transform.position;

            if (vehicalMoveMode == VehicalMoveMode.Straight && usePhysicsFall)
            {
                if (_rb != null)
                    _rb.bodyType = RigidbodyType2D.Dynamic;
            }

            StartMovement();
        }

        private void StartMovement()
        {
            if (_isBroken) return;

            switch (vehicalMoveMode)
            {
                case VehicalMoveMode.Straight:
                    MoveStraight();
                    break;
                case VehicalMoveMode.Curve:
                    MoveCurve();
                    break;
            }
        }

        private void MoveStraight()
        {
            _moveTween = transform.DOMove(pointB, MoveDuration)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(WaitTime)
                .SetTarget(transform)
                .OnStepComplete(FlipDirection);
        }

        private void MoveCurve()
        {
            Vector3 mid = (_pointA + pointB) / 2f;
            Vector3 dir = (pointB - _pointA).normalized;
            Vector3 perp = new Vector3(-dir.y, dir.x, 0f);
            Vector3 controlPoint = mid + perp * curveHeight;

            Vector3[] path = new Vector3[] { controlPoint, pointB };

            _moveTween = transform.DOPath(path, MoveDuration, PathType.CatmullRom)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(WaitTime)
                .SetTarget(transform)
                .OnStepComplete(FlipDirection);
        }

        private void FlipDirection()
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }

        #region Break / Spear Hit

        public override void BreakObstacle()
        {
            if (ObstacleType == ObstacleType.NonDestroyable) return;
            if (_isBroken) return;

            _isBroken = true;

            DOTween.Kill(transform);
            _moveTween = null;

            if (_rb != null)
            {
                _rb.bodyType = RigidbodyType2D.Dynamic;
                _rb.gravityScale = 1f;
            }

            ReleaseEnemy();
        }

        #endregion

        #region Enemy Attachment (Ballon-style)

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_enemyObject != null) return;
            if (!collision.gameObject.CompareTag(GameTags.TEnemy)) return;

            var enemy = collision.gameObject.GetComponent<CharacterBase>();
            if (enemy == null || !enemy.IsScaleReady) return;

            _enemyObject = collision.gameObject;
            IgnoreEnemyCollision(true);
            _enemyObject.transform.position = enemyPos.position;
            // enemy.ChangeState(CharacterState.Driving);
            enemy.IsCanChangeState = false;
            _enemyObject.transform.SetParent(enemyPos);
            SetEnemyPhysics(false);
        }

        private void ReleaseEnemy()
        {
            if (_enemyObject == null) return;

            var enemy = _enemyObject.GetComponent<CharacterBase>();
            if (enemy != null)
            {
                enemy.IsCanChangeState = true;
                enemy.ChangeState(CharacterState.Idle);
            }

            IgnoreEnemyCollision(false);

            var root = GameObject.Find(KeyPrefs.LevelRootName);
            _enemyObject.transform.SetParent(root != null ? root.transform : null);

            SetEnemyPhysics(true);
        }

        private void IgnoreEnemyCollision(bool ignore)
        {
            if (_enemyObject == null) return;
            var enemyCollider = _enemyObject.GetComponent<Collider2D>();
            var vehicalCollider = GetComponent<Collider2D>();
            if (enemyCollider != null && vehicalCollider != null)
                Physics2D.IgnoreCollision(enemyCollider, vehicalCollider, ignore);
        }

        private void SetEnemyPhysics(bool reset)
        {
            if (_enemyObject == null) return;
            var rb = _enemyObject.GetComponent<Rigidbody2D>();
            if (rb == null) return;
           

            if (reset)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 1f;
            }
            else
            {
                rb.bodyType = RigidbodyType2D.Static;
                rb.velocity = Vector2.zero;
                rb.gravityScale = 0f;
            }
        }

        #endregion

        #region Gizmos

        protected new void OnDrawGizmos()
        {
            Vector3 start = Application.isPlaying ? _pointA : transform.position;
            Vector3 end = pointB;

            if (start == end) return;

            switch (vehicalMoveMode)
            {
                case VehicalMoveMode.Straight:
                    DrawStraightGizmo(start, end);
                    break;
                case VehicalMoveMode.Curve:
                    DrawCurveGizmo(start, end);
                    break;
            }

            // Draw enemy pos
            if (enemyPos != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(enemyPos.position, 0.2f);
            }
        }

        private void DrawStraightGizmo(Vector3 start, Vector3 end)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(start, 0.15f);
            Gizmos.DrawSphere(end, 0.15f);
        }

        private void DrawCurveGizmo(Vector3 start, Vector3 end)
        {
            Vector3 mid = (start + end) / 2f;
            Vector3 dir = (end - start).normalized;
            Vector3 perp = new Vector3(-dir.y, dir.x, 0f);
            Vector3 controlPoint = mid + perp * curveHeight;

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(start, 0.15f);
            Gizmos.DrawSphere(end, 0.15f);
            Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
            Gizmos.DrawSphere(controlPoint, 0.1f);

            Gizmos.color = Color.magenta;
            Vector3 prev = start;
            for (int i = 1; i <= curveSegments; i++)
            {
                float t = (float)i / curveSegments;
                Vector3 p = QuadraticBezier(start, controlPoint, end, t);
                Gizmos.DrawLine(prev, p);
                prev = p;
            }
        }

        private Vector3 QuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            float u = 1f - t;
            return u * u * a + 2f * u * t * b + t * t * c;
        }

        #endregion
    }
}
