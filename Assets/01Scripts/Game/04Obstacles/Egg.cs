using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DongA.Characters;
using UnityEngine;
namespace DongA.Obstacles
{
    /// <summary>
    /// A simple obstacle that can be broken by the player.
    /// </summary>
    public class Egg : ObstacleBase
    {
        [SerializeField] private GameObject _topEgg;
        [SerializeField] private GameObject _enemyPosition;
        [SerializeField] private float topEggMoveDistance = 1f;
        [SerializeField] private float topEggMoveDuration = 1f;

        private GameObject _enemyObject;

        public float TopEggMoveDistance { get => topEggMoveDistance; set => topEggMoveDistance = value; }
        public float TopEggMoveDuration { get => topEggMoveDuration; set => topEggMoveDuration = value; }
        public Vector2 EnemyLocalPosition
        {
            get => _enemyPosition != null ? (Vector2)_enemyPosition.transform.localPosition : Vector2.zero;
            set { if (_enemyPosition != null) _enemyPosition.transform.localPosition = value; }
        }

        public Egg(ObstaclesMoveType moveType, float moveDistance, float moveDuration, float waitTime) : base(moveType, moveDistance, moveDuration, waitTime)
        {
        }
        void Start()
        {
            base.Move();
            OpenEgg();
        }

        private void OpenEgg()
        {
            if(moveType == ObstaclesMoveType.Physical) return;
            if (_topEgg != null)
            {
                var sequence = DOTween.Sequence().SetTarget(_topEgg);
                sequence.Append(_topEgg.transform.DOMoveY(_topEgg.transform.position.y + topEggMoveDistance, topEggMoveDuration).SetEase(Ease.Linear));
                sequence.SetLoops(-1, LoopType.Yoyo).SetDelay(WaitTime);
            }
        }
        public override void BreakObstacle()
        {
            
        }


        private void OnCollisionEnter2D(Collision2D collision)
        {
            if(moveType == ObstaclesMoveType.Physical) return;
            if(_enemyObject != null) return;
            if (collision.gameObject.CompareTag(GameTags.TEnemy))
            {
                if (collision.otherCollider.gameObject != _topEgg)
                {
                    return;
                }
                var enemy = collision.gameObject.GetComponent<CharacterBase>();
                if (enemy == null || !enemy.IsScaleReady) return;

                _enemyObject = collision.gameObject;
                _enemyObject.transform.position = _enemyPosition.transform.position;
                // enemy.ChangeState(CharacterState.Hang_idle);
                // enemy.IsCanChangeState = false;
                _enemyObject.transform.SetParent(_enemyPosition.transform);
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