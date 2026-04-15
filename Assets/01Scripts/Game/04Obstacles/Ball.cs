using DG.Tweening;
using DongA.GameDatas;
using DongA.Obstacles;
using UnityEngine;

namespace DongA.Game
{
    public enum BallMoveMode
    {
        Straight,
        Curve
    }

    public class Ball : ObstacleBase
    {
        [Header("Ball Settings")]
        [SerializeField] private BallSO ballSO;
        [SerializeField] private BallMoveMode ballMoveMode = BallMoveMode.Straight;
        [SerializeField] private int BallID;
        [SerializeField] private Vector3 pointB;
        [SerializeField] private float curveHeight = 2f;
        [SerializeField] private int curveSegments = 20;
        
        public BallMoveMode BallMoveModeType { get => ballMoveMode; set => ballMoveMode = value; }
        public Vector3 PointB { get => pointB; set => pointB = value; }
        public float CurveHeight { get => curveHeight; set => curveHeight = value; }
        public int BallId { get => BallID; set => BallID = value; }

        private Vector3 _pointA;
        private Tween _moveTween;
        private bool _isBroken;
        private SpriteRenderer _spriteRenderer;

        public Ball(ObstaclesMoveType moveType, float moveDistance, float moveDuration, float waitTime)
            : base(moveType, moveDistance, moveDuration, waitTime)
        {
        }

        void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateBallSprite();
            _pointA = transform.position;
            StartMovement();
        }

        private void StartMovement()
        {
            if (_isBroken) return;

            switch (ballMoveMode)
            {
                case BallMoveMode.Straight:
                    MoveStraight();
                    break;
                case BallMoveMode.Curve:
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
                .SetTarget(transform);
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
                .SetTarget(transform);
        }

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
            }
            
        }

        protected new void OnDrawGizmos()
        {
            Vector3 start = Application.isPlaying ? _pointA : transform.position;
            Vector3 end = pointB;

            if (start == end) return;

            switch (ballMoveMode)
            {
                case BallMoveMode.Straight:
                    DrawStraightGizmo(start, end);
                    break;
                case BallMoveMode.Curve:
                    DrawCurveGizmo(start, end);
                    break;
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

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(start, 0.15f);
            Gizmos.DrawSphere(end, 0.15f);
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawSphere(controlPoint, 0.1f);

            Gizmos.color = Color.yellow;
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

        private void UpdateBallSprite()
        {
            if (_spriteRenderer == null || ballSO == null || ballSO.ballDatas == null) return;
            foreach (var data in ballSO.ballDatas)
            {
                if (data.id == BallID)
                {
                    _spriteRenderer.sprite = data.ballSprite;
                    return;
                }
            }
        }
    }
}
