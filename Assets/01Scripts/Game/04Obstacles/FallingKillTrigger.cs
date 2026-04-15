using DongA.Characters;
using UnityEngine;

namespace DongA.Obstacles
{
    /// <summary>
    /// Gắn vào object có Rigidbody2D. Khi object rơi và va chạm ở bound dưới thì kill enemy.
    /// Hỗ trợ cả CircleCollider2D (dễ tunnel) bằng cách bổ sung cast check trong FixedUpdate.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class FallingKillTrigger : MonoBehaviour
    {
        [Tooltip("Velocity.y âm tối thiểu để xác nhận object đang rơi (đơn vị: units/s)")]
        [SerializeField] private float minFallSpeed = 0.5f;

        [Tooltip("Vùng tolerance tính từ bottom bound lên (để tránh miss khi collider không hoàn toàn sát đáy)")]
        [SerializeField] private float bottomTolerance = 0.1f;

        private Rigidbody2D _rb;
        private Collider2D _col;
        private readonly Collider2D[] _overlapResults = new Collider2D[8];

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
            // Bật continuous để giảm tunnel khi rơi nhanh
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void FixedUpdate()
        {
            // Chỉ cast khi đang rơi và là CircleCollider2D (dễ bị miss nhất)
            if (_rb.velocity.y > -minFallSpeed) return;
            if (_col is not CircleCollider2D circle) return;

            Vector2 origin = (Vector2)circle.bounds.center;

            // Dùng OverlapCircle tại vị trí hiện tại để bắt các case đã xuyên qua
            int overlapCount = Physics2D.OverlapCircleNonAlloc(origin, circle.radius + bottomTolerance, _overlapResults);
            for (int i = 0; i < overlapCount; i++)
            {
                TryKill(_overlapResults[i]);
            }
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (_rb.velocity.y > -minFallSpeed) return;
            TryKill(col.collider);
        }

        private void TryKill(Collider2D target)
        {
            if (!target.TryGetComponent(out CharacterBase character))
                character = target.GetComponentInParent<CharacterBase>();

            if (character == null) return;
            if (character.IsSpawnInvincible) return;
            if (character.IsDead) return;

            character.DeadNormal();
        }
    }
}
