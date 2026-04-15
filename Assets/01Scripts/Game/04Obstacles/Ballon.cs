using DG.Tweening;
using DongA.Characters;
using DongA.Obstacles;
using Spine.Unity;
using UnityEngine;

namespace DongA.Obstacles
{
    public class Ballon : ObstacleBase
    {
        [Header("Enemy target Settings")]
        [SerializeField] private Transform enemyFollowTarget;
        private GameObject _enemyObject;

        public Transform EnemyFollowTarget { get => enemyFollowTarget; set => enemyFollowTarget = value; }

        public Ballon(ObstaclesMoveType moveType, float moveDistance, float moveDuration, float waitTime) : base(moveType, moveDistance, moveDuration, waitTime)
        {
            MoveType = moveType;
            MoveDistance = moveDistance;
            MoveDuration = moveDuration;
            WaitTime = waitTime;
        }

        void Start()
        {
            Move();
        }

        public override void BreakObstacle()
        {
            DOTween.Kill(transform);
            if(_enemyObject != null)
            {
                var enemy = _enemyObject.GetComponent<CharacterBase>();
                if(enemy != null)
                {
                    enemy.IsCanChangeState = true;
                    enemy.ChangeState(CharacterState.Idle);
                }
                var root = GameObject.Find(KeyPrefs.LevelRootName);
                if(root != null)
                {
                    _enemyObject.transform.SetParent(root.transform);
                }
                else
                {
                    _enemyObject.transform.SetParent(null);
                }
                SetEnemyData(true);
            }
            base.BreakObstacle();
        }

        public override void Move()
        {
            base.Move();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if(_enemyObject != null) return;
            if(collision.gameObject.CompareTag(GameTags.TEnemy))
            {
                var enemy = collision.gameObject.GetComponent<CharacterBase>();
                if (enemy == null || !enemy.IsScaleReady) return;

                _enemyObject = collision.gameObject;
                _enemyObject.transform.position = EnemyFollowTarget.position;
                enemy.ChangeState(CharacterState.Hang_idle);
                enemy.IsCanChangeState = false;
                _enemyObject.transform.SetParent(EnemyFollowTarget);
                SetEnemyData(false);
            }
        }

        private void SetEnemyData(bool reset)
        {
            if(_enemyObject == null) return;
            var rb = _enemyObject.GetComponent<Rigidbody2D>();
            if(rb != null)
            {
                if(reset)
                {
                    rb.gravityScale = 1f;
                }
                else
                {
                    rb.velocity = Vector2.zero;
                    rb.gravityScale = 0f;
                }
            }
        }
    }
}
