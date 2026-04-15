using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DongA.Obstacles
{
    public class Fan : ObstacleBase
    {
        [Header("Wind Settings")]
        [SerializeField] private Vector2 colliderSize = new Vector2(2f, 2f);
        [SerializeField] private Transform windAreaTransform; 
        [SerializeField] private float windForce = 12f;
        [SerializeField] private float smoothTime = 0.3f; // Thời gian để đạt lực tối đa
        [SerializeField] private float maxVelocity = 10f; // Giới hạn tốc độ tối đa
        [SerializeField] private LayerMask affectedLayers;
        [Header("Gravity Override")]
        [SerializeField] private float enemyGravityScale = 0.05f;

        private Dictionary<Rigidbody2D, float> _savedGravityScale;
        private Dictionary<Rigidbody2D, float> _currentForceMultiplier;

        public Vector2 ColliderSize { get => colliderSize; set => colliderSize = value; }
        public float WindForce { get => windForce; set => windForce = value; }
        public Transform WindAreaTransform { get => windAreaTransform; set => windAreaTransform = value; }

        public Fan(ObstaclesMoveType moveType, float moveDistance, float moveDuration, float waitTime) : base(moveType, moveDistance, moveDuration, waitTime)
        {
            MoveType = moveType;
            MoveDistance = moveDistance;
            MoveDuration = moveDuration;
            WaitTime = waitTime;
        }
        void Start()
        {
            _savedGravityScale = new Dictionary<Rigidbody2D, float>();
            _currentForceMultiplier = new Dictionary<Rigidbody2D, float>();
        }

        public override void BreakObstacle()
        {
            ResetGravity();
            base.BreakObstacle();
        }
        void FixedUpdate()
        {
            CheckColliderOverlap();
        }
        private void CheckColliderOverlap()
        {
            if (WindAreaTransform == null)
            {
                Debug.LogWarning("WindAreaTransform is not assigned.");
                return;
            }
            Collider2D[] overlappingColliders = Physics2D.OverlapBoxAll(WindAreaTransform.position, ColliderSize, WindAreaTransform.rotation.eulerAngles.z, affectedLayers);
            
            // Track which rigidbodies are currently in the wind zone
            HashSet<Rigidbody2D> currentInZone = new HashSet<Rigidbody2D>();
            
            foreach (var collider in overlappingColliders)
            {
                var rb = collider.attachedRigidbody;
                if (rb == null || rb.isKinematic) continue;
                
                currentInZone.Add(rb);
                
                // Save original gravity and apply reduced gravity when entering wind zone
                if (!_savedGravityScale.ContainsKey(rb))
                {
                    _savedGravityScale[rb] = rb.gravityScale;
                    rb.gravityScale = enemyGravityScale;
                }
                
                // Smooth ramp up force multiplier
                if (!_currentForceMultiplier.ContainsKey(rb))
                    _currentForceMultiplier[rb] = 0f;
                
                _currentForceMultiplier[rb] = Mathf.MoveTowards(_currentForceMultiplier[rb], 1f, Time.fixedDeltaTime / smoothTime);
                
                // Calculate wind direction based on fan rotation
                Vector2 windDirection = WindAreaTransform.up;
                Vector2 force = windDirection * WindForce * _currentForceMultiplier[rb];
                
                float currentVelocityInWindDir = Vector2.Dot(rb.velocity, windDirection);
                if (currentVelocityInWindDir < maxVelocity)
                {
                    rb.AddForce(force, ForceMode2D.Force);
                }
            }
            
            // Reset multiplier and restore gravity for rigidbodies that left the zone
            List<Rigidbody2D> toRemove = new List<Rigidbody2D>();
            if(_currentForceMultiplier == null) return;
            foreach (var kvp in _currentForceMultiplier)
            {
                if (!currentInZone.Contains(kvp.Key))
                    toRemove.Add(kvp.Key);
            }
            foreach (var rb in toRemove)
            {
                _currentForceMultiplier.Remove(rb);
            }
        }
        private void ResetGravity()
        {
            foreach (var kvp in _savedGravityScale)
            {
                var rb = kvp.Key;
                if (rb != null)
                {
                    rb.gravityScale = kvp.Value;
                }
            }
            _savedGravityScale.Clear();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(WindAreaTransform.position, WindAreaTransform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(ColliderSize.x, ColliderSize.y, 0.1f));
            Gizmos.matrix = oldMatrix;
        }

    }
}