using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DongA.Events;
using DongA.GameDatas;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DongA.UI
{
    public enum ShopPage { None, Skin, SpearSkin, TrailSkin }

    /// <summary>
    /// Shop 3 trang: Skin / SpearSkin / TrailSkin.
    /// Grid 3 cột: [Default] [Common0] [Legendary0]
    ///             [Ads0]    [Common1] [Legendary1] ...
    /// </summary>
    public class ShopManager : PopupBase
    {
        [Header("Canvas Referents")]
        [SerializeField] private Canvas _backgroundCanvas;
        [SerializeField] private Canvas _mainCanvas;
        [Header("Buttons & UI")]
        [SerializeField] private Button back;
        [SerializeField] private Button _removeAdsButton;
        [Header("Data")]
        [SerializeField] private AllSkinDataSO allSkinDataSO;
        [SerializeField] private AllSpearSkinDataSO allSpearSkinDataSO;
        [SerializeField] private AllTrailSkinDataSO allTrailSkinDataSO;
        [SerializeField] private ShopItemUIThemeSO shopItemUIThemeSO;

        [Header("Grid")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform skinGridContent;
        [SerializeField] private Transform spearSkinGridContent;

        [Header("Prefabs")]
        [SerializeField] private GameObject skinItemPrefab;
        [SerializeField] private GameObject spearSkinItemPrefab;
        [SerializeField] private GameObject emptySlotPrefab;

        [Header("Buttons")]
        [SerializeField] private Button SkinTabButton;
        [SerializeField] private Button SpearSkinTabButton;
        [SerializeField] private Button TrailSkinTabButton;

        [Header("Tab")]
        [SerializeField] private Image tabImage;
        [SerializeField] private Sprite skinTabSprite;
        [SerializeField] private Sprite spearSkinTabSprite;
        [SerializeField] private Sprite trailSkinTabSprite;

        [SerializeField] private TextMeshProUGUI coinAmountText;

        [Header("Transition")]
        [SerializeField] private float itemAppearDelay = 0.05f;
        [SerializeField] private float itemAppearDuration = 0.2f;

        [System.Obsolete("Handled by PopupManager stack")]
        public bool ReturnToPausedOnClose { get; set; }

        private ShopPage _currentPage = ShopPage.None;
        private Coroutine _transitionCoroutine;

        public override void ShowPopup()
        {
            InitializePopup();
            GamePrefs.IsGamePaused = true;
            Time.timeScale = 0f;
            GameEvents.OnOpenShop?.Invoke();
        }

        public override void InitializePopup()
        {
            if (_backgroundCanvas != null) _backgroundCanvas.worldCamera = Camera.main;
            if (_mainCanvas != null)
            {
                _mainCanvas.worldCamera = Camera.main;
                _mainCanvas.sortingLayerID = SortingLayer.NameToID(GameTags.SLUIPopup);
            }
        }

        void Awake()
        {
            InitializePopup();

            SkinTabButton.onClick.AddListener(OpenSkinPage);
            SpearSkinTabButton.onClick.AddListener(OpenSpearSkinPage);
            TrailSkinTabButton.onClick.AddListener(OpenTrailSkinPage);
            back.onClick.AddListener(CloseShop);
            _removeAdsButton.onClick.AddListener(OpenNoAdsPopup);

            OnNoAdsChange(GamePrefs.IsNoAds);
        }
        void OnEnable()
        {
            GameEvents.OnCloseShop += CloseShop;
            GameEvents.OnNoAdsChanged += OnNoAdsChange;
        }
        void OnDestroy()
        {
            GameEvents.OnCloseShop -= CloseShop;
            GameEvents.OnNoAdsChanged -= OnNoAdsChange;
            GameEvents.HideMrecUI?.Invoke();
        }
        private void OpenNoAdsPopup()
        {
            GlobalValues.IAP_Location = ParameterValue.shop;
            UIEvent.OnShowPopup?.Invoke(PopupType.NoAds);
            GameEvents.HideMrecUI();
            AnalyticsNavigation.Button(ParameterValue.shop, ParameterValue.no_ads_button);
            //Destroy(gameObject);
        }

        private void CloseShop()
        {
            AnalyticsNavigation.Button(ParameterValue.shop, ParameterValue.close);
            UIEvent.OnClosePopupButtonClicked?.Invoke();
        }

        private void Start()
        {
            InitializeDefaultSkins();
            OpenPage(ShopPage.Skin);
        }

        private void InitializeDefaultSkins()
        {
            if (allSkinDataSO != null && allSkinDataSO.AllSkinDataList != null && allSkinDataSO.AllSkinDataList.Count > 0)
            {
                var first = allSkinDataSO.AllSkinDataList[0];
                GamePrefs.SetCharacterSkinOwned(first.SkinNameID);
                if (GamePrefs.CurrentCharacterSkinID == "Normal")
                {
                    GamePrefs.CurrentCharacterSkinID = first.SkinNameID;
                    GamePrefs.CurrentCharacterSkinName = first.SkinName;
                }
            }

            if (allSpearSkinDataSO != null && allSpearSkinDataSO.AllSpearSkinDataList != null && allSpearSkinDataSO.AllSpearSkinDataList.Count > 0)
            {
                var first = allSpearSkinDataSO.AllSpearSkinDataList[0];
                GamePrefs.SetSpearSkinOwned(first.SkinNameID);
                if (GamePrefs.CurrentSpearSkinID == "Normal")
                {
                    GamePrefs.CurrentSpearSkinID = first.SkinNameID;
                    GamePrefs.CurrentSpearSkinName = first.SkinName;
                    GamePrefs.CurrentSpearType = (int)first.SpearType;
                }
            }

            if (allTrailSkinDataSO != null && allTrailSkinDataSO.AllTrailSkinDataList != null && allTrailSkinDataSO.AllTrailSkinDataList.Count > 0)
            {
                var first = allTrailSkinDataSO.AllTrailSkinDataList[0];
                GamePrefs.SetTrailSkinOwned(first.SkinNameID);
                if (GamePrefs.CurrentTrailSkinID == "Normal")
                {
                    GamePrefs.CurrentTrailSkinID = first.SkinNameID;
                    GamePrefs.CurrentTrailSkinName = first.SkinName;
                }
            }
        }

        // Gán vào OnClick của các button tab
        public void OpenSkinPage()
        {
            OpenPage(ShopPage.Skin);
            AnalyticsNavigation.Button(ParameterValue.shop,ParameterValue.shop_skin_tab);
        }
        public void OpenSpearSkinPage()
        {
            OpenPage(ShopPage.SpearSkin);
            AnalyticsNavigation.Button(ParameterValue.shop, ParameterValue.shop_spear_tab);
        }
        public void OpenTrailSkinPage()
        {
            OpenPage(ShopPage.TrailSkin);
            AnalyticsNavigation.Button(ParameterValue.shop, ParameterValue.shop_trail_tab);
        }

        private void OpenPage(ShopPage page)
        {
            if (_currentPage == page) return;
            _currentPage = page;
            UpdateTabVisual();

            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);

            // Destroy old items synchronously so new tab starts clean immediately
            foreach (Transform child in skinGridContent)
                Destroy(child.gameObject);
            foreach (Transform child in spearSkinGridContent)
                Destroy(child.gameObject);

            _transitionCoroutine = StartCoroutine(TransitionGrid());
        }

        private void UpdateTabVisual()
        {
            var isSkin = true;
            switch (_currentPage)
            {
                case ShopPage.Skin:
                    tabImage.sprite = skinTabSprite;
                    isSkin = true;
                    break;
                case ShopPage.SpearSkin:
                    tabImage.sprite = spearSkinTabSprite;
                    isSkin = false;
                    break;
                case ShopPage.TrailSkin:
                    tabImage.sprite = trailSkinTabSprite;
                    isSkin = true;
                    break;
            }
            var sequence = DOTween.Sequence().SetUpdate(true);
            sequence.Append(tabImage.transform.DOScale(1.01f, 0.05f).SetEase(Ease.Linear));
            sequence.Append(tabImage.transform.DOScale(1f, 0.05f).SetEase(Ease.Linear));
            UpdateScrollViewState(isSkin);
        }
        private void UpdateScrollViewState(bool isSkin)
        {
            if(scrollRect != null)
            {
                if (isSkin)
                {
                    scrollRect.content = skinGridContent.GetComponent<RectTransform>();
                }
                else
                {
                    scrollRect.content = spearSkinGridContent.GetComponent<RectTransform>();
                }
            }
        }

        private IEnumerator TransitionGrid()
        {
            yield return null; // Wait one frame for Destroy to complete

            List<GameObject> newItems = GenerateGridItems();

            foreach (var item in newItems)
            {
                var rt = item.GetComponent<RectTransform>();
                rt.localScale = Vector3.zero;

                rt.DOScale(1f, itemAppearDuration).SetEase(Ease.OutBack).SetUpdate(true);

                yield return new WaitForSecondsRealtime(itemAppearDelay);
            }

            _transitionCoroutine = null;
        }

        private List<GameObject> GenerateGridItems()
        {
            var spawnedItems = new List<GameObject>();
            List<ISkinData> allItems = GetCurrentPageItems();
            if (allItems == null || allItems.Count == 0) return spawnedItems;

            bool isSpearPage = _currentPage == ShopPage.SpearSkin;
            Transform content = isSpearPage ? spearSkinGridContent : skinGridContent;
            GameObject prefab = isSpearPage ? spearSkinItemPrefab : skinItemPrefab;

            foreach (var item in allItems)
            {
                GameObject go = Instantiate(prefab, content);
                go.GetComponent<ShopItemUI>().Setup(item, shopItemUIThemeSO.shopItemUITheme);
                spawnedItems.Add(go);
            }

            return spawnedItems;
        }

        private List<ISkinData> GetCurrentPageItems() => _currentPage switch
        {
            ShopPage.Skin      => allSkinDataSO.AllSkinDataList.ConvertAll(x => (ISkinData)x),
            ShopPage.SpearSkin => allSpearSkinDataSO.AllSpearSkinDataList.ConvertAll(x => (ISkinData)x),
            ShopPage.TrailSkin => allTrailSkinDataSO.AllTrailSkinDataList.ConvertAll(x => (ISkinData)x),
            _                  => null
        };

        private void OnNoAdsChange(bool value)
        {
            _removeAdsButton.gameObject.SetActive(!value);
        }

    }
}
