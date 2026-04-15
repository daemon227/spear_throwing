using DongA.Characters;
using DongA.Obstacles;
using GameBase;
using Spine.Unity;
using UnityEngine;

namespace Game.Obstacles
{
    public class WashingMachine : ObstacleBase
    {
        [SerializeField] private CharacterTypeInWashingMachine characterTypeInWashingMachine;
        public CharacterTypeInWashingMachine CharacterTypeInWashingMachine
        {
            get => characterTypeInWashingMachine;
            set => characterTypeInWashingMachine = value;
        }

        [SerializeField] private CharacterPrefabAllSO washingMachineSO;

        [Header("Spawn Settings")]
        [SerializeField] private int numberCharacterInWashingMachine = 3;
        [SerializeField] private float scaleCharacterInWashingMachine = 0.15f;
        [SerializeField] private float spawnRadius = 0.5f;
        [SerializeField] private float spawnForce = 5f;

        public int NumberCharacterInWashingMachine
        {
            get => numberCharacterInWashingMachine;
            set => numberCharacterInWashingMachine = value;
        }
        public float ScaleCharacterInWashingMachine
        {
            get => scaleCharacterInWashingMachine;
            set => scaleCharacterInWashingMachine = value;
        }
        private SkeletonAnimation _skeletonAnimation;
        public void Start()
        {
            _skeletonAnimation = GetComponent<SkeletonAnimation>();
            ApplyAnimByType();
            if (GameManager.Instance != null)
            {
                for (int i = 0; i < numberCharacterInWashingMachine; i++)
                    GameManager.Instance.IncreaseEnemyCount();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _skeletonAnimation = GetComponent<SkeletonAnimation>();
            ApplyAnimByType();
        }
#endif

        private void ApplyAnimByType()
        {
            if (_skeletonAnimation == null) return;
            string animName = characterTypeInWashingMachine switch
            {
                CharacterTypeInWashingMachine.Pickle => AnimValue.WashingMachineWithPickle,
                CharacterTypeInWashingMachine.Banana => AnimValue.WashingMachineWithBanana,
                CharacterTypeInWashingMachine.Grape => AnimValue.WashingMachineWithGrape,
                CharacterTypeInWashingMachine.Watermelon => AnimValue.WashingMachineWithWatermelon,
                _ => null
            };
            if (!string.IsNullOrEmpty(animName))
                _skeletonAnimation.AnimationName = animName;
        }

        public WashingMachine(ObstaclesMoveType moveType, float moveDistance, float moveDuration, float waitTime) : base(moveType, moveDistance, moveDuration, waitTime)
        {
        }

        public override void BreakObstacle()
        {
            SpawnCharacter();
            base.BreakObstacle();
        }

        private void SpawnCharacter()
        {
            if (washingMachineSO == null) return;

            GameObject characterPrefab = characterTypeInWashingMachine switch
            {
                CharacterTypeInWashingMachine.Pickle => washingMachineSO.picklePrefab,
                CharacterTypeInWashingMachine.Banana => washingMachineSO.bananaPrefab,
                CharacterTypeInWashingMachine.Grape => washingMachineSO.grapePrefab,
                CharacterTypeInWashingMachine.Watermelon => washingMachineSO.watermelonPrefab,
                _ => null
            };

            if (characterPrefab == null) return;

            for (int i = 0; i < numberCharacterInWashingMachine; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
                Vector3 spawnPos = transform.position + (Vector3)randomOffset;

                GameObject spawnedObj = Instantiate(characterPrefab, spawnPos, Quaternion.identity);
                spawnedObj.transform.SetParent(transform.parent);
                spawnedObj.transform.localScale = Vector3.one * scaleCharacterInWashingMachine;

                Rigidbody2D spawnedRb = spawnedObj.GetComponent<Rigidbody2D>();
                if (spawnedRb != null)
                {
                    Vector2 explodeDir = ((Vector2)(spawnPos - transform.position)).normalized;
                    if (explodeDir == Vector2.zero)
                        explodeDir = Random.insideUnitCircle.normalized;
                    spawnedRb.AddForce(explodeDir * spawnForce, ForceMode2D.Impulse);
                }

                CharacterBase character = spawnedObj.GetComponent<CharacterBase>();
                if (character != null)
                {
                    character.ChangeToRandomIdleAnim();
                    character.SetSpawnInvincible(0.3f);
                }

            }
        }

    }
    public enum CharacterTypeInWashingMachine
    {
        Pickle,
        Banana,
        Grape,
        Watermelon
    }
}
