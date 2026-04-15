using DG.Tweening;
using DongA.Events;
using GameBase;
using UnityEngine;

namespace DongA.Obstacles
{
    public class ObstacleBase : MonoBehaviour
    {
        [SerializeField] private ObstacleType obstacleType = ObstacleType.None;
        [SerializeField] protected GameObject breakEffectPrefab;
        [SerializeField] private AudioClip breakSound;

        [SerializeField] protected ObstaclesMoveType moveType = ObstaclesMoveType.None;
        [SerializeField] protected float moveDistance = 0f;
        [SerializeField] protected float moveDuration = 0f;
        [SerializeField] protected float waitTime = 0f;

        protected Rigidbody2D _rb;
        public ObstacleType ObstacleType { get => obstacleType; set => obstacleType = value; }
        public float MoveDistance { get => moveDistance; set => moveDistance = value; }
        public ObstaclesMoveType MoveType { get => moveType; set => moveType = value; }
        public float MoveDuration { get => moveDuration; set => moveDuration = value; }
        public float WaitTime { get => waitTime; set => waitTime = value; }

        public ObstacleBase(ObstaclesMoveType moveType, float moveDistance, float moveDuration, float waitTime)
        {
            MoveType = moveType;
            MoveDistance = moveDistance;
            MoveDuration = moveDuration;
            WaitTime = waitTime;
        }

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if(_rb != null)
            {
                _rb.bodyType = RigidbodyType2D.Static;
            }
        }

        void OnEnable()
        {
            GameEvents.OnMultiSliceEffectStart += PauseMovement;
            GameEvents.OnMultiSliceEffectCompleted += ResumeMovement;
        }

        void OnDisable()
        {
            GameEvents.OnMultiSliceEffectStart -= PauseMovement;
            GameEvents.OnMultiSliceEffectCompleted -= ResumeMovement;
        }

        private void PauseMovement()
        {
            transform.DOPause();
        }

        private void ResumeMovement()
        {
            transform.DOPlay();
        }

        public virtual void BreakObstacle()
        {
            if(ObstacleType == ObstacleType.NonDestroyable)
            {
                return;
            }
            if (breakEffectPrefab != null)
            {
                GameEvents.OnSpawnEffect?.Invoke(breakEffectPrefab, transform.position, Color.white);
            }
            if (breakSound != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySoundAtPoint(breakSound, Camera.main.transform.position, 0.8f);
            }
            Destroy(gameObject);
        }
        public virtual void Move()
        {
            var sequence = DOTween.Sequence().SetTarget(transform);
            switch (MoveType)
            {
                case ObstaclesMoveType.Vertical:
                    // if ( _rb != null)
                    // {
                    //     _rb.constraints = RigidbodyConstraints2D.FreezePositionX;
                    //     _rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
                    // }
                    sequence.Append(transform.DOMoveY(transform.position.y + MoveDistance, MoveDuration).SetEase(Ease.Linear));
                    sequence.SetLoops(-1, LoopType.Yoyo).SetDelay(WaitTime);
                    break;
                case ObstaclesMoveType.Horizontal:
                    // if ( _rb != null)
                    // {
                    //     _rb.constraints = RigidbodyConstraints2D.FreezePositionY;
                    //     _rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
                    // }
                    sequence.Append(transform.DOMoveX(transform.position.x + MoveDistance, MoveDuration).SetEase(Ease.Linear));
                    sequence.SetLoops(-1, LoopType.Yoyo).SetDelay(WaitTime);
                    break;
                case ObstaclesMoveType.None:
                    // if (_rb != null)
                    // {
                    //     _rb.constraints = RigidbodyConstraints2D.FreezeAll;
                    // }
                    break;
                case ObstaclesMoveType.Physical:
                    if (_rb != null)
                    {
                        _rb.bodyType = RigidbodyType2D.Dynamic;
                    }
                    break;
                default:
                    // No movement
                    break;
            }
        }
        protected void OnDrawGizmos()
        {
            if (moveType == ObstaclesMoveType.None || moveDistance == 0f)
                return;

            Gizmos.color = Color.red;
            Vector3 start = transform.position;
            Vector3 end = start;
            switch (moveType)
            {
                case ObstaclesMoveType.Vertical:
                    end += Vector3.up * moveDistance;
                    break;
                case ObstaclesMoveType.Horizontal:
                    end += Vector3.right * moveDistance;
                    break;
            }
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(start, 0.1f);
            Gizmos.DrawSphere(end, 0.1f);
        }
    }

    public enum ObstacleType
    {
        None,
        Destroyable,
        NonDestroyable,
        Pin,
        
    }
    public enum ObstaclesMoveType
    {
        None,
        Vertical,
        Horizontal,
        Physical
    }
}