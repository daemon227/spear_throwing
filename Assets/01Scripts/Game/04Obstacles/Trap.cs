using System.Collections;
using System.Collections.Generic;
using DongA.Characters;
using UnityEngine;

namespace DongA.Obstacles
{
    /// <summary>
    /// Base class for traps that can be placed in the level.
    /// </summary>
    public class Trap : ObstacleBase
    {
        [SerializeField] private Collider2D _dameCollider;
        [SerializeField] private Collider2D _noDameCollider;
        public Trap(ObstaclesMoveType moveType, float moveDistance, float moveDuration, float waitTime) : base(moveType, moveDistance, moveDuration, waitTime)
        {
        }
        void Start()
        {
            Move();
        }

        public override void Move()
        {
            base.Move();
        }
        public override void BreakObstacle()
        {
            //nothing
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if(collision.otherCollider == _noDameCollider)
            {
                return;
            }
            if (collision.gameObject.CompareTag(GameTags.TEnemy))
            {
                var character = collision.gameObject.GetComponentInParent<CharacterBase>();
                if (character != null)
                {
                    character.DeadNormal();
                }
            }
        }

    }
}