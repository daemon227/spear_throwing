using System.Collections;
using System.Collections.Generic;
using DongA.Events;
using DongA.Obstacles;
using GameBase;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DongA.UI
{
    public class PopupWin : PopupBase
    {
        [Header("Button Settings")]
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button claimX2Button;
        [SerializeField] private TextMeshProUGUI coinClaimText;
        [SerializeField] private Image watchAdsIcon;
        [SerializeField] private Button openShop;

        [Header("Graphic Settings")]
        [SerializeField] private SkeletonGraphic skeletonGraphic;
        [SerializeField] private ParticleSystem particle;

        [Header("Coin Claim Effects")]
        [SerializeField] private Transform coinUI;
        [SerializeField] private CoinAnim coinAnim;
        [SerializeField] private int claimX2CoinAmount = 100;
        [SerializeField] private float coinAnimDelay = 2f;

        [Header("Settings")]
        [SerializeField] private float delayBeforShowNextButton;
        [SerializeField] private int freeClaimLimit = 2;

        private const string KeyFreeClaimCount = "FreeClaimX2Count";
        private bool IsFree => PlayerPrefs.GetInt(KeyFreeClaimCount, 0) < freeClaimLimit;
        public static bool IsNoInternet => Application.internetReachability == NetworkReachability.NotReachable;

        private void Awake()
        {
            nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
            if (claimX2Button != null) claimX2Button.onClick.AddListener(OnClaimX2ButtonClicked);
            if (openShop != null) openShop.onClick.AddListener(OnOpenShopClicked);
            UIEvent.OnSetWinButtonsInteractable += SetButtonsInteractable;
            InitializePopup();
        }

        private void OnDestroy()
        {
            UIEvent.OnSetWinButtonsInteractable -= SetButtonsInteractable;
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (nextLevelButton != null) nextLevelButton.interactable = interactable;
            if (claimX2Button != null) claimX2Button.interactable = interactable;
            if (openShop != null) openShop.interactable = interactable;
        }

        void OnOpenShopClicked()
        {
            if (UIManager.Instance != null)
            {
                GameEvents.HideMrecUI();
                UIManager.Instance.OpenShopFromPopup();
            }
        }

        public override void InitializePopup()
        {
            canvas.worldCamera = Camera.main;
            backgroundCanvas.worldCamera = Camera.main;
            var coinCanvas = coinAnim.GetComponent<Canvas>();
            if(coinCanvas != null)
            {
                coinCanvas.worldCamera = Camera.main;
            }
            if(RemoteConfig.Instance != null)
            {
                claimX2CoinAmount = RemoteConfig.Instance.CoinClaimWinLevel;
            }
        }

        void Start()
        {
            Settup();
            StartCoroutine(ShowNextButton());
            if (VibrationsManager.Instance != null) VibrationsManager.Instance.TriggerSuccess();
        }

        void PlayCoinAnim(Button fromButton, int amount, System.Action onComplete)
        {
            nextLevelButton.interactable = false;
            if (claimX2Button != null) claimX2Button.interactable = false;

            if (coinAnim != null && coinUI != null)
                coinAnim.PlayFromWorldPos(fromButton.transform.position, coinUI, amount);

            StartCoroutine(DelayThenInvoke(coinAnimDelay, onComplete));
        }

        private IEnumerator DelayThenInvoke(float seconds, System.Action action)
        {
            yield return new WaitForSecondsRealtime(seconds);
            action?.Invoke();
        }

        void OnNextLevelButtonClicked()
        {
            if (IsNoInternet && !GamePrefs.IsNoAds
                && RemoteConfig.Instance.AdsConfig.Interstitial.Enable
                && GamePrefs.Level >= RemoteConfig.Instance.AdsConfig.Interstitial.Level)
            {
                UIEvent.OnShowPopup(PopupType.NoInternet);
                return;
            }
            AdsUtils.ShowInterstitial("popup_win");
            AnalyticsNavigation.Button("popup_win", ParameterValue.next_level);
            UIEvent.OnNextLevelButtonClicked?.Invoke();
            UIEvent.OnClosePopupButtonClicked?.Invoke();
        }

        void OnClaimX2ButtonClicked()
        {
            AnalyticsNavigation.Button("popup_win", ParameterValue.claim_x2_coin);

            if (IsFree)
            {
                PlayerPrefs.SetInt(KeyFreeClaimCount, PlayerPrefs.GetInt(KeyFreeClaimCount, 0) + 1);
                PlayerPrefs.Save();
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlaySoundAtPoint(SoundManager.Instance.soundSO.coinCollect, Camera.main.transform.position, 1);
                PlayCoinAnim(claimX2Button, claimX2CoinAmount, () =>
                {
                    AnalyticsResouces.InCome(ParameterValue.win_level, ParameterValue.coin_recive_by_watch_ads, ResourceType.Coin, "coin", claimX2CoinAmount);
                    UIEvent.OnNextLevelButtonClicked?.Invoke();
                    UIEvent.OnClosePopupButtonClicked?.Invoke();
                });
                return;
            }

            if (IsNoInternet)
            {
                UIEvent.OnShowPopup(PopupType.NoInternet);
                return;
            }

            AdsUtils.ShowReward((success, location, adInfo) =>
            {
                if (success)
                {
                    if(SoundManager.Instance != null)
                        SoundManager.Instance.PlaySoundAtPoint(SoundManager.Instance.soundSO.coinCollect, Camera.main.transform.position, 1);
                    PlayCoinAnim(claimX2Button, claimX2CoinAmount, () =>
                    {
                        AnalyticsResouces.InCome(ParameterValue.win_level, ParameterValue.coin_recive_by_watch_ads, ResourceType.Coin, "coin", claimX2CoinAmount);
                        UIEvent.OnNextLevelButtonClicked?.Invoke();
                        UIEvent.OnClosePopupButtonClicked?.Invoke();
                    });
                    return;
                }
                else
                {
                    UIEvent.OnNextLevelButtonClicked?.Invoke();
                    UIEvent.OnClosePopupButtonClicked?.Invoke();
                    return;
                }
            }, "popup_win", "coin");
        }

        void Settup()
        {
            if (particle != null)
            {
                // StartCoroutine(UpdateParticleUnscaled());
                particle.Play();
            }
            if(coinClaimText  != null)
            {
                coinClaimText.text = claimX2CoinAmount.ToString();
            }

            if (watchAdsIcon != null) watchAdsIcon.gameObject.SetActive(!IsFree);

            if (skeletonGraphic != null)
            {
                skeletonGraphic.Initialize(true);
                StartCoroutine(UpdateSpineUnscaled());
                claimX2Button.gameObject.SetActive(true);
            }
        }

        // private IEnumerator UpdateParticleUnscaled()
        // {
        //     particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        //     while (particleSystem != null)
        //     {
        //         particleSystem.Simulate(Time.unscaledDeltaTime, true, false);
        //         yield return null;
        //     }
        // }

        private IEnumerator UpdateSpineUnscaled()
        {
            while (skeletonGraphic != null)
            {
                float dt = Time.unscaledDeltaTime;
                skeletonGraphic.AnimationState.Update(dt);
                skeletonGraphic.AnimationState.Apply(skeletonGraphic.Skeleton);
                skeletonGraphic.Skeleton.UpdateWorldTransform();
                yield return null;
            }
        }
        private IEnumerator ShowNextButton()
        {
            yield return new WaitForSecondsRealtime(delayBeforShowNextButton);
            nextLevelButton.gameObject.SetActive(true);
        }

    }
}