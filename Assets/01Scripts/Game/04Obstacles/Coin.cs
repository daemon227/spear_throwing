using DG.Tweening;
using DongA.GameDatas;
using GameBase;
using UnityEngine;

namespace DongA.Obstacles
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Coin : ObstacleBase
    {
        [SerializeField] private int coinValue = 10;
        [SerializeField] private GameObject coinEffectPrefab;
        public int CoinId { get; set; }

        void OnEnable()
        {
            GameEvents.OnMultiSliceEffectCompleted += RecieveCoin;
        }

        void OnDisable()
        {
            GameEvents.OnMultiSliceEffectCompleted -= RecieveCoin;
        }

        public Coin(ObstaclesMoveType moveType, float moveDistance, float moveDuration, float waitTime)
            : base(moveType, moveDistance, moveDuration, waitTime) { }

        public override void BreakObstacle()
        {
            GamePrefs.MarkCoinCollected(GamePrefs.Level, CoinId);

            if (coinEffectPrefab != null)
            {
                GameObject effect = Instantiate(coinEffectPrefab);
                if(SoundManager.Instance != null)
                {
                    var audioSource = effect.GetComponent<AudioSource>();
                    if(audioSource == null)
                    {
                        audioSource = effect.AddComponent<AudioSource>();
                    }
                    SoundManager.Instance.PlaySoundOnShot(audioSource, SoundManager.Instance.soundSO.coinBreak);
                }
                var coinAnim = effect.GetComponentInChildren<CoinAnim>();
                if (coinAnim != null)
                {
                    coinAnim.PlaceCoinCollectAtWorldPos(transform.position, coinValue);
                    Destroy(effect, 2f);
                }
            }
            GlobalValues.Coins_Collected += coinValue;
            AnalyticsResouces.InCome(ParameterValue.in_game, ParameterValue.coin_recive_manner_in_level, ResourceType.Coin, "coin",coinValue);
            gameObject.SetActive(false);
            Destroy(gameObject, 2);
        }
        private void RecieveCoin()
        {
            BreakObstacle();
        }
    }
}
