using DongA.Characters;
using GameBase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DongA.Obstacles
{
    public class Gas : ObstacleBase
    {
        [SerializeField] private float radiusImpact = 3f;
        [SerializeField] private float explosionForce = 5f;
        private bool hasExploded = false;
        public Gas(ObstaclesMoveType moveType, float moveDistance, float moveDuration, float waitTime) : base(moveType, moveDistance, moveDuration, waitTime)
        {
        }
        public override void BreakObstacle()
        {
            if (hasExploded) return;
            hasExploded = true;

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySoundAtPoint(SoundManager.Instance.soundSO.boom, Camera.main.transform.position);
            }

            // Apply explosion force
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radiusImpact, LayerMask.GetMask(GameTags.LObstacle, GameTags.LEnemy));
            foreach (var col in hitColliders)
                {
                    if (col == this.GetComponent<Collider2D>()) continue;
                if (col.CompareTag(GameTags.TObstacle))
                {
                    var obstacle = col.GetComponentInParent<ObstacleBase>();
                    if (obstacle != null && obstacle != this)
                    {
                        obstacle.BreakObstacle();
                    }
                }
                else if (col.CompareTag(GameTags.TEnemy))
                {
                    var character = col.GetComponentInParent<CharacterBase>();
                    if (character != null)
                    {
                        character.DeadByExplosion();
                    }
                }
            }

            // if (obstaclesToBreak.Count > 0 || charactersToKill.Count > 0)
            // {
            //     StartCoroutine(HandleExplosionTargets(obstaclesToBreak, charactersToKill));
            // }

            base.BreakObstacle();
        }

        public override void Move()
        {
            base.Move();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radiusImpact);
        }

        private IEnumerator HandleExplosionTargets(List<ObstacleBase> obstacles, List<CharacterBase> characters)
        {
            yield return null; // wait one frame to avoid deep recursive calls

            foreach (var o in obstacles)
            {
                if (o != null)
                    o.BreakObstacle();
            }

            foreach (var c in characters)
            {
                if (c != null)
                    c.DeadByExplosion();
            }
        }
    }
}

