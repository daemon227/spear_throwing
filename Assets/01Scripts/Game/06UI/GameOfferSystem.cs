using System.Collections.Generic;
using DG.Tweening;
using DongA.Events;
using DongA.GameDatas;
using DongA.Powerups;
using DongA.SpearLogic;
using UnityEngine;
using UnityEngine.UI;

namespace DongA.UI
{
    public class GameOfferSystem : MonoBehaviour
    {
        [Header("UI")]
        
        [SerializeField] private Button _showOfferButton;
        [SerializeField] private GameObject _offerButtonShowingGroup;
        [SerializeField] private Button _watchAdsButton;
        [SerializeField] private Image _skinImage;
        [SerializeField] private GameObject _adIcon;

        [Header("Data")]
        [SerializeField] private AllSkinDataSO allSkinDataSO;
        [SerializeField] private AllTrailSkinDataSO allTrailSkinDataSO;
        [SerializeField] private AllPowerupSO allPowerupSO;

        [Header("Probability")]
        // x: % Skin (character), y: % Powerup (trail skin + actual powerup)
        [SerializeField] private Vector2 percentSpawnBetweenSkinAndPowerup = new Vector2(70, 30);

        [Header("Animation")]
        [SerializeField] private float _pulseScale = 0.01f;
        [SerializeField] private float _pulseDuration = 0.5f;
        [SerializeField] private float _slideOutOffset = 700f;
        [SerializeField] private float _hideDuration = 0.25f;

        // ─── State ───────────────────────────────────────────────────────

        private enum OfferType { None, CharacterSkin, TrailSkin, Powerup }

        private OfferType _currentOfferType = OfferType.None;
        private SkinData _currentSkinOffer;
        private TrailSkinData _currentTrailOffer;
        private PowerUpData _currentPowerupOffer;

        private string _originalTrailSkin;
        private string _originalTrailSkinName;
        private RectTransform _groupRect;
        private Vector2 _groupOriginPos;
        private RectTransform _showButtonRect;
        private Vector2 _showButtonOriginPos;

        private const string KeyFreeClaimOffer = "KeyFreeClaimOffer";
        [SerializeField] private int freeClaimLimit = 2;
        private bool IsFree => PlayerPrefs.GetInt(KeyFreeClaimOffer, 0) < freeClaimLimit;

        // ─── Lifecycle ───────────────────────────────────────────────────

        void Awake()
        {
            _groupRect = _offerButtonShowingGroup.GetComponent<RectTransform>();
            _groupOriginPos = _groupRect.anchoredPosition;

            _showButtonRect = _showOfferButton.GetComponent<RectTransform>();
            _showButtonOriginPos = _showButtonRect.anchoredPosition;

            _watchAdsButton.onClick.AddListener(OnWatchAdsClicked);
            _showOfferButton.onClick.AddListener(OnShowOfferButtonClicked);

            _offerButtonShowingGroup.SetActive(false);
            _showOfferButton.gameObject.SetActive(false);
        }

        void OnEnable()
        {
            GameEvents.OnLevelLoadComplete += OnLevelLoaded;
            GameEvents.OnLevelStartLoading += OnLevelStartLoading;
            GameEvents.OnSpearDragging += OnSpearDragging;
        }

        void OnDisable()
        {
            GameEvents.OnLevelLoadComplete -= OnLevelLoaded;
            GameEvents.OnLevelStartLoading -= OnLevelStartLoading;
            GameEvents.OnSpearDragging -= OnSpearDragging;
        }

        // ─── Level Events ────────────────────────────────────────────────

        private void OnLevelLoaded()
        {
            if(RemoteConfig.Instance.LevelUnlockOffer<GamePrefs.Level)
            PickOffer();
        }

        private void OnLevelStartLoading()
        {
            if(RemoteConfig.Instance.LevelUnlockOffer<GamePrefs.Level)
            RestoreTrailIfNeeded();
            ClearOffer();
            HideButton(instant: true);
        }

        private void OnSpearDragging()
        {
            if(RemoteConfig.Instance.LevelUnlockOffer<GamePrefs.Level)
            HideButton(instant: false);
        }

        // ─── Offer Picking ───────────────────────────────────────────────

        private void PickOffer()
        {
            float total = percentSpawnBetweenSkinAndPowerup.x + percentSpawnBetweenSkinAndPowerup.y;
            float roll = Random.Range(0f, total);
            bool trySkinFirst = roll < percentSpawnBetweenSkinAndPowerup.x;

            if (trySkinFirst)
            {
                if (TryPickCharacterSkin()) return;
                if (TryPickPowerupOffer()) return;
            }
            else
            {
                if (TryPickPowerupOffer()) return;
                if (TryPickCharacterSkin()) return;
            }
        }

        private bool TryPickCharacterSkin()
        {
            if (allSkinDataSO == null) return false;

            var available = new List<SkinData>();
            foreach (var skin in allSkinDataSO.AllSkinDataList)
            {
                if (skin.skinType == SkinType.Ads && !GamePrefs.IsCharacterSkinOwned(skin.skinNameID))
                    available.Add(skin);
            }
            if (available.Count == 0) return false;

            _currentSkinOffer = available[Random.Range(0, available.Count)];
            _currentOfferType = OfferType.CharacterSkin;
            _skinImage.sprite = _currentSkinOffer.skinSprite;
            ShowButton();
            return true;
        }

        // Powerup offer = trail skin hoặc actual powerup, pick random
        private bool TryPickPowerupOffer()
        {
            bool hasTrail = allTrailSkinDataSO != null && allTrailSkinDataSO.AllTrailSkinDataList != null
                            && allTrailSkinDataSO.AllTrailSkinDataList.Count > 0;
            bool hasPowerup = allPowerupSO != null && allPowerupSO.PowerUpDatas != null
                              && allPowerupSO.PowerUpDatas.Count > 0;

            if (!hasTrail && !hasPowerup) return false;

            // Roll giữa trail vs actual powerup (50/50 nếu cả hai đều có)
            bool pickTrail = hasTrail && (!hasPowerup || Random.value < 0.5f);

            if (pickTrail) return TryPickTrailSkin();
            return TryPickActualPowerup();
        }

        private bool TryPickTrailSkin()
        {
            var list = allTrailSkinDataSO.AllTrailSkinDataList;
            if (list == null || list.Count == 0) return false;

            // Ưu tiên Ads trail chưa sở hữu; nếu không có thì lấy tất cả
            var ads = new List<TrailSkinData>();
            foreach (var t in list)
                if (t.skinType == SkinType.Ads && !GamePrefs.IsTrailSkinOwned(t.skinNameID))
                    ads.Add(t);

            var pool = ads.Count > 0 ? ads : new List<TrailSkinData>(list);
            _currentTrailOffer = pool[Random.Range(0, pool.Count)];
            _currentOfferType = OfferType.TrailSkin;
            _skinImage.sprite = _currentTrailOffer.skinSprite;
            ShowButton();
            return true;
        }

        private bool TryPickActualPowerup()
        {
            var list = allPowerupSO.PowerUpDatas;
            if (list == null || list.Count == 0) return false;

            _currentPowerupOffer = list[Random.Range(0, list.Count)];
            _currentOfferType = OfferType.Powerup;
            _skinImage.sprite = _currentPowerupOffer.powerupImage;
            ShowButton();
            return true;
        }

        // ─── Ads ─────────────────────────────────────────────────────────

        private void OnWatchAdsClicked()
        {
            if(GamePrefs.IsUIBlocked || GamePrefs.IsShopTutorialPanelActive) return;
            if (_currentOfferType == OfferType.None) return;
            AnalyticsNavigation.Button(ParameterValue.in_game, ParameterValue.offer_button);

            if (IsFree)
            {
                PlayerPrefs.SetInt(KeyFreeClaimOffer, PlayerPrefs.GetInt(KeyFreeClaimOffer, 0) + 1);
                PlayerPrefs.Save();
                GrantReward();
                return;
            }

            if (AdsUtils.IsNoInternet)
            {
                UIEvent.OnShowPopup(PopupType.NoInternet);
                return;
            }

            AdsUtils.ShowReward((success, location, adInfo) =>
            {
                if (success) GrantReward();
            }, ParameterValue.offer_button, _currentOfferType.ToString());
        }

        // ─── Grant Reward ────────────────────────────────────────────────
        private void GrantReward()
        {
            switch (_currentOfferType)
            {
                case OfferType.CharacterSkin: GrantCharacterSkin(); break;
                case OfferType.TrailSkin:     GrantTrailSkin();     break;
                case OfferType.Powerup:       GrantActualPowerup(); break;
            }

            _showOfferButton.gameObject.SetActive(false);
            ClearOffer();
            HideButton(instant: false);
        }

        private void GrantCharacterSkin()
        {
            if (_currentSkinOffer == null) return;
            GamePrefs.SetCharacterSkinOwned(_currentSkinOffer.skinNameID);
            GamePrefs.CurrentCharacterSkinID = _currentSkinOffer.skinNameID;
            GamePrefs.CurrentCharacterSkinName = _currentSkinOffer.skinName;

            AnalyticsResouces.InCome(ParameterValue.offer_button, ParameterValue.watch_ads, ResourceType.Skin, "Character_Skin_"+_currentSkinOffer.skinName, 1);

            GameEvents.OnCharacterSkinChosen?.Invoke();
        }

        private void GrantTrailSkin()
        {
            if (_currentTrailOffer == null) return;
            _originalTrailSkin = GamePrefs.CurrentTrailSkinID;
            _originalTrailSkinName = GamePrefs.CurrentTrailSkinName;
            GamePrefs.CurrentTrailSkinID = _currentTrailOffer.skinNameID;
            GamePrefs.CurrentTrailSkinName = _currentTrailOffer.skinName;

            AnalyticsResouces.InCome(ParameterValue.offer_button, ParameterValue.watch_ads, ResourceType.Skin, "Trail_Skin_"+_currentTrailOffer.skinName, 1);
            GameEvents.OnTrailSkinChosen?.Invoke();
        }

        private void GrantActualPowerup()
        {
            if (_currentPowerupOffer == null || _currentPowerupOffer.powerupPrefab == null) return;

            var playerSpear = FindPlayerSpear();
            var powerupGO = Instantiate(_currentPowerupOffer.powerupPrefab, Vector3.zero, Quaternion.identity);
            if (powerupGO.TryGetComponent<PowerupBase>(out var powerup))
                powerup.PowerupReceived(playerSpear != null ? playerSpear.gameObject : null, Vector2.up);
            AnalyticsResouces.InCome(ParameterValue.offer_button, ParameterValue.watch_ads, ResourceType.Powerup, "Powerup_"+_currentPowerupOffer.PowerupName, 1);
        }

        // ─── Trail Restore ───────────────────────────────────────────────

        private void RestoreTrailIfNeeded()
        {
            if (string.IsNullOrEmpty(_originalTrailSkin)) return;
            GamePrefs.CurrentTrailSkinID = _originalTrailSkin;
            GamePrefs.CurrentTrailSkinName = _originalTrailSkinName;
            GameEvents.OnTrailSkinChosen?.Invoke();
            _originalTrailSkin = null;
            _originalTrailSkinName = null;
        }

        // ─── Animation ───────────────────────────────────────────────────

        private void ShowButton()
        {
            DOTween.Kill(_groupRect);
            DOTween.Kill(_showButtonRect);
            DOTween.Kill(_watchAdsButton.transform);

            _groupRect.anchoredPosition = _groupOriginPos;
            _offerButtonShowingGroup.SetActive(true);
            _showOfferButton.gameObject.SetActive(false);

            _adIcon.SetActive(!IsFree);

            _watchAdsButton.transform.localScale = Vector3.one;
            _watchAdsButton.transform
                .DOScale(1f + _pulseScale, _pulseDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        private void HideButton(bool instant)
        {
            bool showMini = _currentOfferType != OfferType.None;

            if (instant)
            {
                DOTween.Kill(_groupRect);
                DOTween.Kill(_showButtonRect);
                DOTween.Kill(_watchAdsButton.transform);
                _groupRect.anchoredPosition = _groupOriginPos;
                _showButtonRect.anchoredPosition = _showButtonOriginPos;
                _offerButtonShowingGroup.SetActive(false);
                _showOfferButton.gameObject.SetActive(false);
                return;
            }

            if (_offerButtonShowingGroup.activeSelf)
            {
                DOTween.Kill(_groupRect);
                DOTween.Kill(_watchAdsButton.transform);
                _groupRect.DOAnchorPosX(_groupOriginPos.x + _slideOutOffset, _hideDuration)
                    .SetEase(Ease.InCubic)
                    .OnComplete(() =>
                    {
                        _offerButtonShowingGroup.SetActive(false);
                        _groupRect.anchoredPosition = _groupOriginPos;
                        if (showMini) ShowMiniButton();
                    });
            }
            else if (showMini && !_showOfferButton.gameObject.activeSelf)
            {
                ShowMiniButton();
            }
        }

        private void ShowMiniButton()
        {
            DOTween.Kill(_showButtonRect);
            _showButtonRect.anchoredPosition = new Vector2(_showButtonOriginPos.x + _slideOutOffset, _showButtonOriginPos.y);
            _showOfferButton.gameObject.SetActive(true);
            _showButtonRect.DOAnchorPosX(_showButtonOriginPos.x, _hideDuration)
                .SetEase(Ease.OutCubic);
        }

        private void OnShowOfferButtonClicked()
        {
            if(GamePrefs.IsUIBlocked || GamePrefs.IsShopTutorialPanelActive) return;
            DOTween.Kill(_showButtonRect);
            _showOfferButton.gameObject.SetActive(false);
            _showButtonRect.anchoredPosition = _showButtonOriginPos;

            DOTween.Kill(_groupRect);
            _groupRect.anchoredPosition = new Vector2(_groupOriginPos.x + _slideOutOffset, _groupOriginPos.y);
            _offerButtonShowingGroup.SetActive(true);
            _groupRect.DOAnchorPosX(_groupOriginPos.x, _hideDuration)
                .SetEase(Ease.OutCubic);

            DOTween.Kill(_watchAdsButton.transform);
            _adIcon.SetActive(!IsFree);
            _watchAdsButton.transform.localScale = Vector3.one;
            _watchAdsButton.transform
                .DOScale(1f + _pulseScale, _pulseDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        // ─── Helpers ─────────────────────────────────────────────────────

        private void ClearOffer()
        {
            _currentOfferType = OfferType.None;
            _currentSkinOffer = null;
            _currentTrailOffer = null;
            _currentPowerupOffer = null;
        }

        private SpearController FindPlayerSpear()
        {
            foreach (var spear in FindObjectsOfType<SpearController>())
            {
                if (!spear.isEnemySpear) return spear;
            }
            return null;
        }
    }
}
