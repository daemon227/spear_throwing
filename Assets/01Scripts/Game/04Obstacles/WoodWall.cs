using System.Collections.Generic;
using DG.Tweening;
using DongA.Characters;
using UnityEngine;

namespace DongA.Obstacles
{
    public class WoodWall : ObstacleBase
    {
        private List<GameObject> _attachedEnemies = new List<GameObject>();

        public WoodWall(ObstaclesMoveType moveType, float moveDistance, float moveDuration, float waitTime) : base(moveType, moveDistance, moveDuration, waitTime)
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
            // DOTween.Kill(transform);
            // RemoveAttachedEnemies();
            // base.BreakObstacle();
        }

        public override void Move()
        {
            base.Move();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                var enemy = collision.gameObject.GetComponent<CharacterBase>();
                if (enemy == null || !enemy.IsScaleReady) return;

                Collider2D myCol = GetComponent<Collider2D>();
                Collider2D otherCol = collision.collider;

                if (otherCol.bounds.min.y >= myCol.bounds.max.y)
                {
                    collision.gameObject.transform.SetParent(transform);
                    if(_attachedEnemies == null)
                    {
                        _attachedEnemies = new List<GameObject>();
                    }
                    if(!_attachedEnemies.Contains(collision.gameObject))
                    {
                        _attachedEnemies.Add(collision.gameObject);
                    }
                }
            }
        }
    }
} 
