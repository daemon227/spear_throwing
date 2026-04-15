using System;
using DG.Tweening;
using DongA.Events;
using DongA.GameDatas;
using DongA.Obstacles;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace DongA.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI coinText;
        public GameObject coinBar;
        [SerializeField] private Button shopButton;

        [SerializeField] private Button openNoAdButton;
        [SerializeField] private GameObject MrecUIPrefab;
        [SerializeField] private CoinAnim iapCoinAnim;

        [Header("Skin Defaults")]
        [SerializeField] private AllSkinDataSO allSkinDataSO;
        [SerializeField] private AllSpearSkinDataSO allSpearSkinDataSO;

        private GameObject currentMrecUI;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitializeDefaultSkins();
        }

        private void InitializeDefaultSkins()
        {
            if (allSkinDataSO != null && allSkinDataSO.AllSkinDataList != null && allSkinDataSO.AllSkinDataList.Count > 0)
            {
                var first = allSkinDataSO.AllSkinDataList[0];
                if (!PlayerPrefs.HasKey(KeyPrefs.CurrentCharacterSkin))
                {
                    GamePrefs.SetCharacterSkinOwned(first.SkinNameID);
                    GamePrefs.CurrentCharacterSkinID = first.SkinNameID;
                }
            }

            if (allSpearSkinDataSO != null && allSpearSkinDataSO.AllSpearSkinDataList != null && allSpearSkinDataSO.AllSpearSkinDataList.Count > 0)
            {
                var first = allSpearSkinDataSO.AllSpearSkinDataList[0];
                if (!PlayerPrefs.HasKey(KeyPrefs.CurrentSpearSkin))
                {
                    GamePrefs.SetSpearSkinOwned(first.SkinNameID);
                    GamePrefs.CurrentSpearSkinID = first.SkinNameID;
                }
                if (!PlayerPrefs.HasKey(KeyPrefs.CurrentSpearType))
                    GamePrefs.CurrentSpearType = (int)first.SpearType;
            }
        }
        void Start()
        {
            UpdateLevelText();
            UpdateCoinText();
            shopButton.onClick.AddListener(OpenShop);
            openNoAdButton.onClick.AddListener(OpenNoAdsPopup);
            OnNoAdsChange(GamePrefs.IsNoAds);
        }
        private void OnEnable()
        {
            GameEvents.OnLevelLoadComplete += UpdateLevelText;
            GameEvents.ShowMrecUI += ShowMrecUI;
            GameEvents.HideMrecUI += HideMrecUI;
            UIEvent.OnClosePopupButtonClicked += HideMrecUI;
            GameEvents.OnNoAdsChanged += OnNoAdsChange;
            GameEvents.OnIAPCoinReward += PlayIAPCoinRewardAnim;
        }
        private void OnDisable()
        {
            GameEvents.OnLevelLoadComplete -= UpdateLevelText;
            GameEvents.ShowMrecUI -= ShowMrecUI;
            GameEvents.HideMrecUI -= HideMrecUI;
            UIEvent.OnClosePopupButtonClicked -= HideMrecUI;
            GameEvents.OnNoAdsChanged -= OnNoAdsChange;
            GameEvents.OnIAPCoinReward -= PlayIAPCoinRewardAnim;

        }
        private void ShowMrecUI(string location)
        {
            currentMrecUI = Instantiate(MrecUIPrefab);
            var mrecUIScript = currentMrecUI.GetComponent<UIMRec>();
            if(mrecUIScript == null) return;
            mrecUIScript.Location = location; 
        }
        private void HideMrecUI()
        {
            if(currentMrecUI !=null)
            {
                Destroy(currentMrecUI);
            }
        }

        private void UpdateLevelText()
        {
            levelText.text = "Level " + (GamePrefs.Level + 1).ToString();
        }
        public void RecieveCoinAnim(int coinAmount)
        {
            UpdateCoinText(coinAmount);
            DOTween.Kill(coinBar.transform);
            coinBar.transform.localScale = Vector3.one;
            coinBar.transform.localRotation = Quaternion.identity;
            var sequence = DOTween.Sequence().SetTarget(coinBar.transform).SetUpdate(true);
            sequence.Append(coinBar.transform.DOScale(1.25f, 0.08f).SetEase(Ease.OutBack));
            // sequence.Join(coinBar.transform.DOPunchRotation(new Vector3(0, 0, 12f), 0.35f, 8, 0.5f));
            sequence.Append(coinBar.transform.DOScale(1f, 0.1f).SetEase(Ease.InBack));
        }

        private void PlayIAPCoinRewardAnim(int coinAmount)
        {
            if (iapCoinAnim != null)
            {
                
                Vector3 startPos = Camera.main.ScreenToWorldPoint(
                    new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 10f));
                iapCoinAnim.PlayFromWorldPos(startPos, coinBar.transform, coinAmount);
            }
            else
            {
                // Fallback: no animation, add coins directly
                GamePrefs.Coin += coinAmount;
            }
        }

        private void UpdateCoinText(int coinAmount = 0)
        {
            GamePrefs.Coin += coinAmount;
            coinText.text = GamePrefs.FormatCoin(GamePrefs.Coin);
        }
        private void OpenShop()
        {
            if (GamePrefs.IsUIBlocked) return;
            if (GameBase.GameManager.Instance != null && GameBase.GameManager.Instance.CurrentGameState != GameBase.GameState.Playing) return;
            AnalyticsNavigation.Button(ParameterValue.main_ui, ParameterValue.shop);
            UIEvent.OnShowPopup?.Invoke(PopupType.Shop);
        }

        public void OpenShopFromPopup()
        {
            if (GamePrefs.IsUIBlocked) return;
            AnalyticsNavigation.Button(ParameterValue.win_level, ParameterValue.shop);
            UIEvent.OnShowPopup?.Invoke(PopupType.Shop);
        }
        private void OpenNoAdsPopup()
        {
            if (GamePrefs.IsUIBlocked || GamePrefs.IsShopTutorialPanelActive) return;
            if (GameBase.GameManager.Instance != null && GameBase.GameManager.Instance.CurrentGameState != GameBase.GameState.Playing) return;
            GlobalValues.IAP_Location = ParameterValue.main_ui;
            UIEvent.OnShowPopup?.Invoke(PopupType.NoAds);
            AdsUtils.HideBanner();
            AnalyticsNavigation.Button(ParameterValue.shop, ParameterValue.no_ads_button);
        }

        private void OnNoAdsChange(bool value)
        {
            openNoAdButton.gameObject.SetActive(!value);
        }

    }
}

