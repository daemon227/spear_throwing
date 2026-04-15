using DG.Tweening;
using DongA.Events;
using UnityEngine;

namespace DongA.Characters
{
    /// <summary>
    /// Enemy that instantly teleports between spawn position (A) and Point B every time the spear is launched.
    /// Point A is the wizard's initial transform position. Only Point B is set in the Level Editor.
    /// </summary>
    public class Wizard1 : CharacterBase
    {
        [Header("Wizard1 - Two Point Teleport")]
        [SerializeField] private Vector2 pointB = new(3f, 0f);
        [SerializeField] private GameObject teleportEffectPrefab;
        [SerializeField] private float spawnInvincibleDuration = 0f;

        public Vector2 PointB { get => pointB; set => pointB = value; }
        private Vector2 _pointA;
        private Vector2 _initScale = Vector2.one;

        private bool _atPointA = true;

        protected override void Start()
        {
            _pointA = transform.position;
            _initScale = transform.localScale;
            base.Start();
        }

        private void OnEnable()
        {
            GameEvents.OnSpearLaunch += OnSpearLaunched;
            GameEvents.OnLevelLoadComplete += SetInitialScale;
            GameEvents.OnMultiSliceEffectStart += OnMultiSliceEffectStart;
            GameEvents.OnMultiSliceEffectCompleted += DeadByVerticalSlice;
            GameEvents.OnCharacterSkinChosen += ChangeSkin;
            GameEvents.OnLevelStartLoading += ResetSkinSpawnCount;
        }

        private void OnDisable()
        {
            GameEvents.OnSpearLaunch -= OnSpearLaunched;
            GameEvents.OnLevelLoadComplete -= SetInitialScale;
            GameEvents.OnMultiSliceEffectStart -= OnMultiSliceEffectStart;
            GameEvents.OnMultiSliceEffectCompleted -= DeadByVerticalSlice;
            GameEvents.OnCharacterSkinChosen -= ChangeSkin;
            GameEvents.OnLevelStartLoading -= ResetSkinSpawnCount;
        }

        private void OnSpearLaunched()
        {
            if (currentState == CharacterState.Dead) return;
            if (_isFrozen) return;

            _atPointA = !_atPointA;
            Vector2 target = _atPointA ? _pointA : pointB;
            DoTeleportTo(target);
        }

        private void DoTeleportTo(Vector2 target)
        {
            StopMoving();

            // Reset physics & collider to active state (giống Wizard.DoTeleport)
            rb.bodyType = RigidbodyType2D.Static;
            boxCollider2D.isTrigger = false;
            boxCollider2D.enabled = true;
            _isHitByTrajectory = false;

            if (teleportEffectPrefab != null)
                GameEvents.OnSpawnEffect?.Invoke(teleportEffectPrefab, boxCollider2D.bounds.center, Color.magenta);

            transform.position = new Vector3(target.x, target.y, transform.position.z);

            // Brief invincibility so spear can't immediately re-hit
            SetSpawnInvincible(spawnInvincibleDuration);

            currentState = CharacterState.Dead;
            ChangeState(CharacterState.Idle);
            Move();

            Vector3 savedScale = _initScale;
            transform.localScale = Vector3.zero;
            transform.DOScale(savedScale, 0.25f).SetEase(Ease.OutBack);
        }
        protected override void ChangeSkin()
        {
            
        }

#if UNITY_EDITOR
        protected new void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            // Point A - cyan sphere (wizard's own position)
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.position, 0.15f);

            // Point B - yellow sphere
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere((Vector3)pointB, 0.15f);

            // Line connecting A and B
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawLine(transform.position, pointB);
        }
#endif
    }
}
