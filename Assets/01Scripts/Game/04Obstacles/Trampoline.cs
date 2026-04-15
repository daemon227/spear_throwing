using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DongA.Obstacles
{
    public class Trampoline : ObstacleBase
    {
        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private LayerMask affectedLayers;
        [Header("Gravity Override")]

        private readonly Dictionary<Rigidbody2D, float> _savedGravityScale = new Dictionary<Rigidbody2D, float>();

        public float JumpForce { get => jumpForce; set => jumpForce = value; }

        public Trampoline(ObstaclesMoveType moveType, float moveDistance, float moveDuration, float waitTime) : base(moveType, moveDistance, moveDuration, waitTime)
        {
            MoveType = moveType;
            MoveDistance = moveDistance;
            MoveDuration = moveDuration;
            WaitTime = waitTime;
        }

            public override void BreakObstacle()
        {
            Debug.Log("Trampoline obstacle broken!");
            ResetGravity();
            base.BreakObstacle();
            // Destroy(gameObject);
        }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            var rb = collision.rigidbody;
            if (rb == null || rb.isKinematic) return;
            Vector2 force = Vector2.up * JumpForce;
            rb.AddForce(force, ForceMode2D.Force);
        }

        // private IEnumerator RestoreGravityAfterDelay(Rigidbody2D rb)
        // {
        //     yield return new WaitForSeconds(gravityDisableDuration);

        //     if (rb != null && _savedGravityScale.TryGetValue(rb, out var originalGravity))
        //     {
        //         rb.gravityScale = originalGravity;
        //         _savedGravityScale.Remove(rb);
        //     }
        // }

        private void ResetGravity()
        {
            if (_savedGravityScale == null || _savedGravityScale.Count == 0) return;
            var nullKeys = new List<Rigidbody2D>();
            foreach (var kvp in _savedGravityScale)
            {
                var rb = kvp.Key;
                if (rb == null)
                {
                    nullKeys.Add(rb);
                    continue;
                }
                rb.gravityScale = kvp.Value;
            }
            // Remove null keys if any
            foreach (var key in nullKeys)
            {
                _savedGravityScale.Remove(key);
            }
            _savedGravityScale.Clear();
        }
    }
}
