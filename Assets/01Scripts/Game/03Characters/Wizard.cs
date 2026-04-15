using DG.Tweening;
using DongA.Events;
using UnityEngine;

namespace DongA.Characters
{
    public class Wizard : CharacterBase
    {
        [Header("Teleport Settings")]
        [SerializeField] private int numberTeleport = 3;
        [SerializeField] private Vector2 boundsMin = new(-3f, -4f);
        [SerializeField] private Vector2 boundsMax = new(3f, 4f);
        [SerializeField] private float spawnInvincibleDuration = 0.5f;
        [SerializeField] private float minTeleportDistance = 2f;
        [SerializeField] private int maxTeleportAttempts = 10;
        [SerializeField] GameObject teleportEffectPrefab;

        public int NumberTeleport { get => numberTeleport; set => numberTeleport = value; }

        private bool _isTeleporting;

        // Returns true if teleport was performed (still has charges)
        private bool TryTeleport()
        {
            if (_isFrozen) return false;
            if (numberTeleport <= 0) return false;
            numberTeleport--;
            DoTeleport();
            GameEvents.OnSpawnEffect?.Invoke(teleportEffectPrefab, boxCollider2D.bounds.center, Color.magenta);
            return true;
        }

        private Vector2 FindTeleportPosition()
        {
            Vector2 currentPos = transform.position;
            Vector2 colliderSize = boxCollider2D.bounds.size;

            for (int i = 0; i < maxTeleportAttempts; i++)
            {
                float x = Random.Range(boundsMin.x, boundsMax.x);
                float y = Random.Range(boundsMin.y, boundsMax.y);
                Vector2 candidate = new(x, y);

                // Must be far enough from current position
                if (Vector2.Distance(candidate, currentPos) < minTeleportDistance)
                    continue;

                // Must not overlap any existing collider
                Collider2D hit = Physics2D.OverlapBox(candidate, colliderSize * 0.9f, 0f);
                if (hit == null || hit == boxCollider2D)
                    return candidate;
            }

            // Fallback: just pick a position that is far enough (ignore overlap check)
            for (int i = 0; i < maxTeleportAttempts; i++)
            {
                float x = Random.Range(boundsMin.x, boundsMax.x);
                float y = Random.Range(boundsMin.y, boundsMax.y);
                Vector2 candidate = new(x, y);
                if (Vector2.Distance(candidate, currentPos) >= minTeleportDistance)
                    return candidate;
            }

            // Last resort: any random position
            return new Vector2(
                Random.Range(boundsMin.x, boundsMax.x),
                Random.Range(boundsMin.y, boundsMax.y));
        }

        private void DoTeleport()
        {
            StopMoving();

            // Reset physics & collider to active state
            rb.bodyType = RigidbodyType2D.Static;
            boxCollider2D.isTrigger = false;
            boxCollider2D.enabled = true;
            _isHitByTrajectory = false;

            // Move to a valid position: far from current pos and not overlapping any object
            Vector2 newPos = FindTeleportPosition();
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);

            // Brief invincibility so spear can't immediately re-hit
            SetSpawnInvincible(spawnInvincibleDuration);

            // Force state transition: set to Dead so ChangeState(Idle) always fires
            currentState = CharacterState.Dead;
            ChangeState(CharacterState.Idle);

            // Resume movement BEFORE scale animation so Move() captures the correct scale
            Move();

            // Appear effect (after Move so scale is correct for SideToSide callbacks)
            Vector3 targetScale = transform.localScale;
            transform.localScale = Vector3.zero;
            transform.DOScale(targetScale, 0.25f).SetEase(Ease.OutBack);
        }

        // --- Override all death entry points ---

        // Called first by ColliderHandler for Basic spear; BasicSpearHandler then calls PinToSpearAtPoint.
        // We block PinToSpearAtPoint via _isTeleporting flag.
        public override void DeadAndPin(Transform spearTransform = null)
        {
            if (TryTeleport()) { _isTeleporting = true; return; }
            base.DeadAndPin(spearTransform);
        }

        public override void PinToSpearAtPoint(Transform spear, Vector2 worldPinPoint)
        {
            if (_isTeleporting) { _isTeleporting = false; return; }
            base.PinToSpearAtPoint(spear, worldPinPoint);
        }


        public override void DeadAndSlice(Vector3 spearDirection)
        {
            if (TryTeleport()) return;
            base.DeadAndSlice(spearDirection);
        }

        public override void DeadByExplosion()
        {
            if (TryTeleport()) return;
            base.DeadByExplosion();
        }

        public override void DeadNormal()
        {
            if (TryTeleport()) return;
            base.DeadNormal();
        }
        public override void DeadByThunder()
        {
            if(TryTeleport()) return;
            base.DeadByThunder();
        }

        protected override void ChangeSkin()
        {
            // base.ChangeSkin();
        }


        // Visualize teleport bounds in Scene view
        protected new void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            Gizmos.color = Color.cyan;
            Vector3 center = new(
                (boundsMin.x + boundsMax.x) * 0.5f,
                (boundsMin.y + boundsMax.y) * 0.5f,
                0f);
            Vector3 size = new(
                Mathf.Abs(boundsMax.x - boundsMin.x),
                Mathf.Abs(boundsMax.y - boundsMin.y),
                0.01f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
