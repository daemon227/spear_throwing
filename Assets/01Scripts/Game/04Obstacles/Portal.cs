using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DongA.Characters;
using GameBase;
using UnityEngine;

namespace DongA.Obstacles
{
    public class Portal : ObstacleBase
    {
        [Header("Portal Settings")]
        [SerializeField] private int _portalID;
        [SerializeField] private int _linkedPortalID;
        [SerializeField] private float _appearDelay = 0.2f;
        [SerializeField] private float _moveSpeed = 1.5f;
        [SerializeField] private float _shrinkDuration = 0.2f;
        [SerializeField] private float _growDuration = 0.25f;
        [SerializeField] private float _invincibleDuration = 0.5f;
        [SerializeField] private GameObject _teleportEffectPrefab;

        private Portal _linkedPortal;
        private Collider2D _collider;

        // Enemy đang trong giai đoạn shrink/teleport/grow - chặn portal này bắt lại
        private HashSet<CharacterBase> _teleportingEnemies;
        // Enemy thuộc portal khác quản lý, đang đi bộ về portal này - chặn portal này bắt
        private HashSet<CharacterBase> _blockedEnemies;

        public int PortalID { get => _portalID; set => _portalID = value; }
        public int LinkedPortalID { get => _linkedPortalID; set => _linkedPortalID = value; }
        public float AppearDelay { get => _appearDelay; set => _appearDelay = value; }
        public float PortalMoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }
        public Portal LinkedPortal => _linkedPortal;

        public Portal(ObstaclesMoveType moveType, float moveDistance, float moveDuration, float waitTime) : base(moveType, moveDistance, moveDuration, waitTime)
        {
        }

        void Awake()
        {
            _teleportingEnemies = new HashSet<CharacterBase>();
            _blockedEnemies = new HashSet<CharacterBase>();
            if (GameManager.Instance != null)
                GameEvents.OnLevelLoadComplete += OnLevelLoadedHandle;
            else
                OnLevelLoadedHandle();
        }

        void OnDisable()
        {
            if (GameManager.Instance != null)
                GameEvents.OnLevelLoadComplete -= OnLevelLoadedHandle;
        }

        private void Start()
        {
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.bodyType = RigidbodyType2D.Kinematic;

            _collider = GetComponent<Collider2D>();
            if (_collider != null)
                _collider.isTrigger = true;
        }

        void OnLevelLoadedHandle()
        {
            var portals = FindObjectsOfType<Portal>();
            foreach (var portal in portals)
            {
                if (portal.PortalID == _linkedPortalID)
                {
                    _linkedPortal = portal;
                    break;
                }
            }
        }

        public override void BreakObstacle() { }
        public override void Move() { }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(GameTags.TEnemy)) return;
            if (_linkedPortal == null) return;

            var character = other.GetComponentInParent<CharacterBase>();
            if (character == null) return;

            // Đang được portal này xử lý rồi
            if (_teleportingEnemies.Contains(character)) return;
            // Thuộc portal kia quản lý, đang đi bộ về đây
            if (_blockedEnemies.Contains(character)) return;

            StartCoroutine(TeleportAndLoop(character, other));
        }

        private IEnumerator TeleportAndLoop(CharacterBase character, Collider2D charCollider)
        {
            _teleportingEnemies.Add(character);

            // yOffset = khoảng cách từ chân character đến tâm portal (áp dụng nhất quán cho cả 2 portal)
            float yOffset = character.transform.position.y - transform.position.y;

            // Dừng chuyển động + tắt trọng lực
            character.StopMoving();
            var charRb = character.GetComponentInChildren<Rigidbody2D>();
            // RigidbodyType2D origBodyType = RigidbodyType2D.Dynamic;
            if (charRb != null)
            {
                // origBodyType = charRb.bodyType;
                charRb.bodyType = RigidbodyType2D.Kinematic;
                charRb.velocity = Vector2.zero;
            }

            // bool origTrigger = charCollider != null && charCollider.isTrigger;
            if (charCollider != null) charCollider.isTrigger = true;

            // 1. Di chuyển đến vị trí portal vào (giữ Y chân character)
            Vector3 entryPos = new Vector3(transform.position.x, transform.position.y + yOffset, 0f);
            float distToEntry = Vector3.Distance(character.transform.position, entryPos);
            if (distToEntry > 0.1f)
            {
                yield return character.transform
                    .DOMove(entryPos, distToEntry / _moveSpeed)
                    .SetEase(Ease.Linear)
                    .WaitForCompletion();
            }

            // 2. Thu nhỏ tại portal vào
            Vector3 originalScale = character.transform.localScale;
            yield return character.transform
                .DOScale(Vector3.zero, _shrinkDuration)
                .SetEase(Ease.InBack)
                .WaitForCompletion();

            // 3. Effect tại portal vào
            if (_teleportEffectPrefab != null)
                GameEvents.OnSpawnEffect?.Invoke(_teleportEffectPrefab, transform.position, Color.cyan);

            // 4. Dịch chuyển đến portal đích (scale = 0), áp dụng cùng offset
            Vector3 exitPos = new Vector3(_linkedPortal.transform.position.x, _linkedPortal.transform.position.y + yOffset, 0f);
            character.transform.position = exitPos;

            _linkedPortal._blockedEnemies.Add(character);

            // 6. Delay trước khi xuất hiện
            if (_appearDelay > 0f)
                yield return new WaitForSeconds(_appearDelay);

            // 7. Effect tại portal ra
            if (_teleportEffectPrefab != null)
                GameEvents.OnSpawnEffect?.Invoke(_teleportEffectPrefab, _linkedPortal.transform.position, Color.cyan);

            // 8. Phóng to tại portal đích
            yield return character.transform
                .DOScale(originalScale, _growDuration)
                .SetEase(Ease.OutBack)
                .WaitForCompletion();

            // 9. Bất tử ngắn
            character.SetSpawnInvincible(_invincibleDuration);

            // 10. Portal này xong giai đoạn teleport
            _teleportingEnemies.Remove(character);

            // 11. Đi bộ từ portal đích về portal này
            Vector3 returnPos = new Vector3(transform.position.x, transform.position.y + yOffset, 0f);
            float distance = Vector3.Distance(exitPos, returnPos);
            yield return character.transform
                .DOMove(returnPos, distance / _moveSpeed)
                .SetEase(Ease.Linear)
                .WaitForCompletion();

            // 12. Đã về đến portal này, gỡ block ở portal đích
            _linkedPortal._blockedEnemies.Remove(character);

            // Restore trigger state trước khi restore bodyType để OnTriggerEnter2D bắt cycle mới
            // if (charCollider != null) charCollider.isTrigger = origTrigger;

            // // 13. Khôi phục trọng lực
            // if (charRb != null)
            //     charRb.bodyType = origBodyType;

            // OnTriggerEnter2D tại portal này sẽ bắt lại → bắt đầu cycle mới
        }
    }
}
