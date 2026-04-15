using GameBase;
using UnityEngine;

namespace DongA.Characters
{
    public class BigCharacter : CharacterBase
    {
        [Header("Big Character Settings")]
        [SerializeField] private GameObject smallCharacterPrefab;
        [SerializeField] private int spawnCount = 3;
        [SerializeField] private float spawnRadius = 0.5f;
        [SerializeField] private float spawnForce = 3f;
        [SerializeField] private float childSizeMultiplier = 0.15f;

        public int SpawnCount { get => spawnCount; set => spawnCount = value; }
        public float SpawnRadius { get => spawnRadius; set => spawnRadius = value; }
        public float SpawnForce { get => spawnForce; set => spawnForce = value; }
        public float ChildSizeMultiplier { get => childSizeMultiplier; set => childSizeMultiplier = value; }
        
        public override void DeadAndPin(Transform spearTransform = null)
        {
            SpawnSmallCharacters();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DecreaseEnemyCount(boxCollider2D.bounds.center);
            }
            if (characterData != null)
            {
                GameEvents.OnDeadEffect?.Invoke(boxCollider2D.bounds.center, characterData.bloodEffectPrefab, characterData.bloodEffectColor, characterData.soulPrefab);
            }
            Destroy(gameObject);
        }

        public override void DeadAndSlice(Vector3 spearDirection)
        {
            SpawnSmallCharacters();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DecreaseEnemyCount(boxCollider2D.bounds.center);
            }
            if (characterData != null)
            {
                GameEvents.OnDeadEffect?.Invoke(boxCollider2D.bounds.center, characterData.bloodEffectPrefab, characterData.bloodEffectColor, characterData.soulPrefab);
            }
            Destroy(gameObject);
        }

        public override void DeadByExplosion()
        {
            SpawnSmallCharacters();
            base.DeadByExplosion();
        }


        public override void DeadByElemental(GameObject deadEffectPrefab = null)
        {
            SpawnSmallCharacters();
            base.DeadByElemental(deadEffectPrefab);
        }
        private void SpawnSmallCharacters()
        {
            if (smallCharacterPrefab == null) return;

            for (int i = 0; i < spawnCount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
                Vector3 spawnPos = transform.position + (Vector3)randomOffset;

                GameObject small = Instantiate(smallCharacterPrefab, spawnPos, Quaternion.identity);
                small.transform.localScale = Vector2.one * childSizeMultiplier;
                small.transform.SetParent(transform.parent);

                Rigidbody2D smallRb = small.GetComponent<Rigidbody2D>();
                if (smallRb != null)
                {
                    Vector2 randomDir = Random.insideUnitCircle.normalized;
                    smallRb.AddForce(randomDir * spawnForce, ForceMode2D.Impulse);
                }


                var smallCharacter = small.GetComponent<CharacterBase>();
                if (smallCharacter != null)
                {
                    smallCharacter.ChangeToRandomIdleAnim();
                    smallCharacter.SetSpawnInvincible(0.3f);
                }

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.IncreaseEnemyCount();
                }
            }

            // Đảm bảo ít nhất 1 enemy con mặc skin (Start/ChangeSkin chạy frame sau)
            CharacterBase.ResetSkinWearerForSpawn();
        }
        public override void DeadNormal()
        {
            SpawnSmallCharacters();
            base.DeadNormal();
        }
        

        public override void DeadByThunder()
        {
            SpawnSmallCharacters();
            base.DeadByThunder();
        }
    }
}
