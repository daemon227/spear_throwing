//#define DEBUG_ADS_INTERNAL
//#define TEST_INTERNAL
using System;
#if ADMOB
using GoogleMobileAds.Api;
#endif
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inwave.Production.Ads
{
    [RequireComponent(typeof(Canvas))]
    public class NativeInterstitialAdmob : NativeInterstitialNetwork
    {
        //[Header("Ad unit id:")]
        //[SerializeField] private string _adUnitIdAndroid = "YOUR_AD_UNIT_ID";
        //[SerializeField] private string _adUnitIdIos = "YOUR_AD_UNIT_ID";

        [Space(10)] [SerializeField] private GameObject _parent;
        [SerializeField] private Image _frame;

        [SerializeField] private RectTransform _container;

        [SerializeField] private Image _imageMainContent;
        [SerializeField] private Image _imageIcon;
        [SerializeField] private TextMeshProUGUI _headlineText;
        [SerializeField] private TextMeshProUGUI _mainText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private bool _enablePriceText = true;
        [SerializeField] private TextMeshProUGUI _adText;
        [SerializeField] private bool _enableAdText = true;
        [SerializeField] private Image _adIcon;
        [SerializeField] private bool _enableAdIcon = true;

        [SerializeField] private Button _buttonClose;

        [SerializeField] private float _microValueRatio = 1000000;

        private bool _isLoading = false;

#if ADMOB
        private NativeAd _nativeAd = null;
#endif
#if ADMOB

        private Canvas _canvas;
        void Start()
        {
            LogTag = "NativeInterstitialAdmob";

            _canvas = GetComponent<Canvas>();
            //
            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.sortingOrder = short.MaxValue;
            

            _buttonClose.onClick.AddListener(Close);
            _adIcon.color = Color.white;
            _adIcon.preserveAspect = true;
            _imageMainContent.color = Color.white;
            _imageMainContent.preserveAspect = true;
            _imageIcon.color = Color.white;
            _imageIcon.preserveAspect = true;

            _parent.SetActive(false);
        }

        public override void Init(string adUnitId)
        {
            _adUnitId = adUnitId;
        }

        private AdNetwork.AdInfo CreateAdInfo()
        {
            AdNetwork.AdInfo adInfo = new AdNetwork.AdInfo
            {
                Format = AdsValues.AD_FORMAT_NATIVE_INTERSTITIAL,
                Platform = AdsValues.AD_PLATFORM_ADMOB,
                UnitId = _adUnitId
            };
            return adInfo;
        }

        #region event handlers

        private void _nativeAd_OnPaidEvent(object sender, AdValueEventArgs e)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            Debug.LogWarning($"_nativeAd_OnPaidEvent: {e.AdValue.Value} {e.AdValue.CurrencyCode}");
#endif
            AdNetwork.AdImpressionData impressionData = new AdNetwork.AdImpressionData()
            {
                Currency = e.AdValue.CurrencyCode,
                UnitName = _adUnitId,
                Format = AdsValues.AD_FORMAT_NATIVE_INTERSTITIAL,
                Platform = AdsValues.AD_PLATFORM_ADMOB,
                Revenue = e.AdValue.Value / _microValueRatio
            };
            AdsEvents.FakeInterstitial.AdPaidEvent(impressionData);
        }

        private void AdLoader_OnNativeAdImpression(object sender, System.EventArgs e)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogWarning("AdLoader_OnNativeAdImpression");
#endif
            var adInfo = CreateAdInfo();

            AdsEvents.FakeInterstitial.AdShowedEvent(adInfo);
        }

        private void AdLoader_OnNativeAdOpening(object sender, System.EventArgs e)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogWarning("AdLoader_OnNativeAdOpening");
#endif
            var adInfo = CreateAdInfo();
            AdsEvents.FakeInterstitial.AdLinkOpenedEvent(adInfo);

            DestroyAd();
            Load();
        }

        private void AdLoader_OnNativeAdClosed(object sender, System.EventArgs e)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogWarning("AdLoader_OnNativeAdClosed");
#endif
            var adInfo = CreateAdInfo();
            AdsEvents.FakeInterstitial.AdLinkClosedEvent(adInfo);
        }

        private void AdLoader_OnNativeAdClicked(object sender, System.EventArgs e)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogWarning("AdLoader_OnNativeAdClicked");
#endif
            var adInfo = CreateAdInfo();
            AdsEvents.FakeInterstitial.AdClickedEvent(adInfo);
        }

        private void HandleNativeAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogWarning("Native ad failed to load: " + args.LoadAdError);
#endif
            _isLoading = false;
            AdNetwork.AdError adError = new AdNetwork.AdError()
            {
                Code = args.LoadAdError.GetCode(),
                Message = args.LoadAdError.GetMessage(),
                UnitId = _adUnitId
            };
            AdsEvents.FakeInterstitial.AdLoadFailedEvent(adError);
        }


        private void HandleNativeAdLoaded(object sender, NativeAdEventArgs args)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogWarning("Native ad loaded.");
#endif
            _isLoading = false;
            this._nativeAd = args.nativeAd;
            _nativeAd.OnPaidEvent += _nativeAd_OnPaidEvent;

            CreateMainContent();
            CreateHeadlineText();
            CreateMainText();
            CreateIcon();

            CreateExtras();

            Resize();
            int ok = _nativeAd.RegisterImageGameObjects(new List<GameObject> { _frame.gameObject });
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            Debug.LogWarning($"OK - {ok}");
#endif
            _parent.SetActive(false);
            AdNetwork.AdInfo adInfo = CreateAdInfo();
            AdsEvents.FakeInterstitial.AdLoadedEvent(adInfo);
        }

        #endregion

        #region Create UI
        void CreateExtras()
        {

            if (_enablePriceText)
            {
                _priceText.gameObject.SetActive(true);
                _priceText.text = _nativeAd.GetPrice();
            }
            else
            {
                _priceText.gameObject.SetActive(false);
            }

            if (_enableAdIcon)
            {
                var logo = _nativeAd.GetAdChoicesLogoTexture();
                if (logo != null)
                {
                    Sprite sprite = Sprite.Create(logo, new Rect(0, 0, logo.width, logo.height),
                        new Vector2(0.5f, 0.5f));
                    _adIcon.sprite = sprite;
                    _adIcon.gameObject.SetActive(true);
                }
                else
                {
                    _adIcon.gameObject.SetActive(false);
                }
            }
            else
            {
                _adIcon.gameObject.SetActive(false);
            }

            if (_enableAdText)
            {
                var adText = _nativeAd.GetAdvertiserText();
                if (adText != null)
                {
                    _adText.gameObject.SetActive(true);
                    _adText.text = adText;
                }
                else
                {
                    _adText.gameObject.SetActive(false);
                }
            }
            else
            {
                _adText.gameObject.SetActive(false);
            }

        }
        void Resize()
        {
            float frameWidth = _frame.rectTransform.sizeDelta.x;
            //float frameHeight = _frame.rectTransform.sizeDelta.y;

            float ratio = Screen.width / frameWidth;
            _container.localScale = new Vector2(ratio, ratio);
        }
        void CreateMainContent()
        {
            var images = _nativeAd.GetImageTextures();
            if (images.Count > 0)
            {
                Texture2D iconTexture = images[0];
                Sprite sprite = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height),
                    new Vector2(0.5f, 0.5f));
                _imageMainContent.sprite = sprite;
            }
        }

        void CreateHeadlineText()
        {

            _headlineText.text = _nativeAd.GetHeadlineText();

        }

        void CreateMainText()
        {

            _mainText.text = _nativeAd.GetBodyText();

        }

        void CreateIcon()
        {
            Texture2D iconTexture = _nativeAd.GetIconTexture();
            Sprite sprite = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));
            _imageIcon.sprite = sprite;
        }
        #endregion

        private string _adUnitId = "ca-app-pub-3940256099942544/2247696110";
        public override void Load()
        {
            if (_isLoading)
            {
                return;
            }
            _isLoading = true;
            AdsEvents.FakeInterstitial.AdRequestLoadEvent();
            base.Load();

            _nativeAd = null;
            AdLoader adLoader = new AdLoader.Builder(_adUnitId)
                .ForNativeAd()
                .Build();

            adLoader.OnNativeAdClicked += AdLoader_OnNativeAdClicked;
            adLoader.OnNativeAdClosed += AdLoader_OnNativeAdClosed;
            adLoader.OnNativeAdOpening += AdLoader_OnNativeAdOpening;
            adLoader.OnNativeAdImpression += AdLoader_OnNativeAdImpression;
            adLoader.OnNativeAdLoaded += this.HandleNativeAdLoaded;
            adLoader.OnAdFailedToLoad += this.HandleNativeAdFailedToLoad;

            adLoader.LoadAd(new AdRequest.Builder().Build());
        }

        public override void Hide()
        {
#if !UNITY_EDITOR
            if (_nativeAd == null)
            {
                return;
            }
#endif
            if (_parent.activeSelf)
            {
                base.Hide();
                _parent.SetActive(false);
            }
        }

        public override void Show()
        {
#if !UNITY_EDITOR
            if (_nativeAd == null)
            {
                return;
            }
#endif
            if (!_parent.activeSelf)
            {
                AdsEvents.FakeInterstitial.AdRequestShowEvent();
                base.Show();
                //Time.timeScale = 0;
                
                _canvas.worldCamera = Camera.main;
                _parent.SetActive(true);
            }
        }

        public override void DestroyAd()
        {
#if !UNITY_EDITOR
            if (_nativeAd == null)
            {
                return;
            }
#endif
            base.DestroyAd();
            _nativeAd.Destroy();
            _nativeAd = null;
            _parent.SetActive(false);

        }

        protected override void Close()
        {
#if !UNITY_EDITOR
            if (_nativeAd == null)
            {
                return;
            }
#endif
            base.Close();
            DestroyAd();
            var adInfo = CreateAdInfo();
            AdsEvents.FakeInterstitial.AdClosedEvent(adInfo);
            Load();
        }

        public override bool IsAvailable()
        {
#if UNITY_EDITOR
            return true;
#else
            return _nativeAd != null;
#endif
        }

        public void ShowInfo()
        {
            var iconTexture = _nativeAd.GetIconTexture();
            var imageTextures = _nativeAd.GetImageTextures();
            var headlineText = _nativeAd.GetHeadlineText();
            var bodyText = _nativeAd.GetBodyText();
            var adChoicesLogoTexture = _nativeAd.GetAdChoicesLogoTexture();
            var advertiserText = _nativeAd.GetAdvertiserText();
            var price = _nativeAd.GetPrice();
            var starRating = _nativeAd.GetStarRating();
            
            Debug.LogWarning(iconTexture);
            Debug.LogWarning(imageTextures.Count);
            Debug.LogWarning(headlineText);
            Debug.LogWarning(bodyText);
            Debug.LogWarning(adChoicesLogoTexture);
            Debug.LogWarning(advertiserText);
            Debug.LogWarning(price);
            Debug.LogWarning(starRating);
        }

#endif
#if DEBUG_ADS && TEST_INTERNAL
        private void OnGUI()
        {
            if (GUI.Button(new Rect(50,Screen.height-100,250,50), "Load Fake Inters"))
            {
                Load();
            }
            if (GUI.Button(new Rect(Screen.width - 300, Screen.height - 100, 250, 50), "Show Fake Inters"))
            {
                Show();
            }
        }
#endif
    }
}

