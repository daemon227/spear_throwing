using System.Collections.Generic;
using DG.Tweening;
using DongA.Characters;
using GameBase;
using UnityEngine;

namespace DongA.Obstacles
{
    public class IceWall : ObstacleBase
    {
        private List<GameObject> _attachedEnemies = new List<GameObject>();

        public IceWall(ObstaclesMoveType moveType, float moveDistance, float moveDuration, float waitTime) : base(moveType, moveDistance, moveDuration, waitTime)
        {
            MoveType = moveType;
            MoveDistance = moveDistance;
            MoveDuration = moveDuration;
            WaitTime = waitTime;
        }

        void Start()
        {
            MoveIceWall();
        }

        public override void BreakObstacle()
        {
            DOTween.Kill(transform);
            RemoveAttachedEnemies();
            // if (SoundManager.Instance != null)
            // {
            //     SoundManager.Instance.PlaySoundAtPoint(SoundManager.Instance.soundSO.iceBreak, Camera.main.transform.position);
            // }
            base.BreakObstacle();
        }

        private void MoveIceWall()
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
                    if( !_attachedEnemies.Contains(collision.gameObject))
                    {
                        _attachedEnemies.Add(collision.gameObject);
                    }
                }
            }
        }

        private void RemoveAttachedEnemies()
        {
            if (_attachedEnemies == null || _attachedEnemies.Count == 0) return;
            foreach (var enemy in _attachedEnemies)
            {
                if (enemy != null)
                {
                    var root = GameObject.Find(KeyPrefs.LevelRootName);
                    if(root != null)
                    {
                        enemy.transform.SetParent(root.transform);
                    }
                    else
                    {
                        enemy.transform.SetParent(null);
                    }
                }
            }
            _attachedEnemies.Clear();
        }
    }

}
