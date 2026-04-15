//#define DEBUG_ADS_INTERNAL
using System;
using System.Collections;
using Inwave.Production.Utils;
using UnityEngine;

namespace Inwave.Production.Ads
{
    //AppOpen Ad
    public partial class AdsManager : MonoBehaviour
    {
#if DEBUG_ADS
        private const string LogTagAdsManagerAppOpen = "AdsManager AppOpen";
#endif
        [Header("+App Open Ad------")]
        [SerializeField]
        private bool _enableAppOpenAd = true;
        [SerializeField]
        private AppOpenAdNetwork _appOpenAdNetwork;
        [SerializeField]
        private float _appOpenShowingInterval = 30;
        [SerializeField] 
        private int _maxTryAppOpenAd = 5;

        [SerializeField] private AdmobImageInterstitial _admobImageInterstitial;

        private double _appOpenTimer = 0;

        private double _appOpenAdLastTime = 0;

        private bool _isAppOpenInitialized = false;


        #region Properties

        public AppOpenAdNetwork AppOpenAd => _appOpenAdNetwork;

        public float AppOpenShowingInterval
        {
            set => _appOpenShowingInterval = value;
        }

        public bool EnableAppOpenAd
        {
            set
            {
                _enableAppOpenAd = value;
                if (_enableAppOpenAd)
                {
                    InitAppOpen();
                }
            }
            get => _enableAppOpenAd;
        }

        public int MaxTryAppOpenAd
        {
            set => _maxTryAppOpenAd = value;
        }

        public bool EnableAdmobImageInterstitial { get; set; } = false;

        #endregion

        private void InitAppOpen()
        {
            StartCoroutine(DelayInit());
            IEnumerator DelayInit()
            {
                yield return null;
                Init();
            }

            void Init()
            {
                _appOpenAdLastTime = Time.realtimeSinceStartupAsDouble;

                if (_isAppOpenInitialized)
                {
                    return;
                }
                if (_enableAppOpenAd && _appOpenAdNetwork != null)
                {
                    _appOpenAdNetwork.Initialize();
                    _appOpenAdNetwork.OnAppStateChanged += AppOpenAd_OnAppStateChanged;

                    _appOpenAdNetwork.OnAdClicked += AppOpenAd_OnAdClicked;
                    _appOpenAdNetwork.OnAdClosed += AppOpenAd_OnAdClosed;
                    _appOpenAdNetwork.OnAdLoaded += AppOpenAd_OnAdLoaded;
                    _appOpenAdNetwork.OnAdShowed += AppOpenAd_OnAdShowed;
                    _appOpenAdNetwork.OnAdShowFailed += AppOpenAd_OnAdShowFailed;
                    _appOpenAdNetwork.OnAdPaid += _appOpenAdNetwork_OnAdPaid;
                    _appOpenAdNetwork.Load();


                    if (EnableAdmobImageInterstitial)
                    {
                        _admobImageInterstitial.Load();
                    }
                    _isAppOpenInitialized = true;
                }
            }
        }

        private void _appOpenAdNetwork_OnAdPaid(AdNetwork.AdImpressionData impressionData)
        {
            OnImpressionSuccessEvent(impressionData);
        }


        #region Event handlers

        

        private void AppOpenAd_OnAppStateChanged(AppOpenAdNetwork.AppState state)
        {
            if (!CanShowAppOpenAd())
            {
                return;
            }
#if DEBUG_ADS &&DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTagAdsManagerAppOpen,$"AppOpenAd_OnAppStateChanged: {state}");
#endif

            AdsEvents.AppOpen.OnAppStateChangedEvent(state);

            var deltaTime = Time.realtimeSinceStartupAsDouble - _appOpenAdLastTime;
            _appOpenAdLastTime = Time.realtimeSinceStartupAsDouble;

            _appOpenTimer += deltaTime;
#if DEBUG_ADS &&DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTagAdsManagerAppOpen, $"AppOpenAd AppOpenTimer: {_appOpenTimer}");
#endif

            if (state == AppOpenAdNetwork.AppState.Foreground)
            {
                if (_appOpenTimer > _appOpenShowingInterval)
                {
                    AdsUtils.ShowAOA();
                }
            }
        }

        public void Show()
        {
            if (!_isAppOpenInitialized)
            {
                return;
            }
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTagAdsManagerAppOpen, $"Show - IsAvailable: {_appOpenAdNetwork.IsAdAvailable}");
#endif
            if (EnableAdmobImageInterstitial)
            {
                if (_admobImageInterstitial != null && _admobImageInterstitial.IsAvailable)
                {
                    _appOpenTimer = 0;
                    _appOpenAdLastTime = Time.realtimeSinceStartupAsDouble;
                    _admobImageInterstitial.Show();
                }
                else
                {
                    ShowMAXAOA();
                    _admobImageInterstitial.Load();
                }
            }
            else
            {
                ShowMAXAOA();
            }
        }

        private void ShowMAXAOA()
        {
            StartCoroutine(_Show());

            IEnumerator _Show()
            {
                if (!_appOpenAdNetwork.IsAdAvailable)
                {
                    _appOpenAdNetwork.Load();
                }
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                LogUtils.LogWarning(LogTagAdsManagerAppOpen, "AppOpenAd_Show");
#endif
                int tryCounter = 0;
                WaitForSeconds wait = new WaitForSeconds(1);
                while (tryCounter < _maxTryAppOpenAd)
                {
                    if (_appOpenAdNetwork.IsAdAvailable)
                    {
                        //reset Timer
                        _appOpenTimer = 0;
                        _appOpenAdLastTime = Time.realtimeSinceStartupAsDouble;

                        _appOpenAdNetwork.Show();
                        break;
                    }
                    tryCounter++;
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                    LogUtils.LogWarning(LogTagAdsManagerAppOpen, $"AppOpenAd_Show_Try: {tryCounter}");
#endif
                    yield return wait;

                }

                if (tryCounter >= _maxTryAppOpenAd)
                {
                    //force to show next time the Game goto Foreground
                    _appOpenTimer = _appOpenShowingInterval;
                    _appOpenAdLastTime = Time.realtimeSinceStartupAsDouble;

                    AdNetwork.AdError adError = new AdNetwork.AdError
                    {
                        Message = AdsValues.AD_ERROR_TWO_SLOW
                    };

                    AdsEvents.AppOpen.OnAdShowFailedEvent(_adInfo, adError);
                }
            }
        }

        private bool _canShowAppOpenAd = true;

        public void SetCanShowAppOpenAdd(bool can)
        {
            _canShowAppOpenAd = can;
        }
        private bool CanShowAppOpenAd()
        {
            bool canShow = _enableAppOpenAd && _canShowAppOpenAd;
            return canShow;
        }

        private void AppOpenAd_OnAdShowFailed(AdNetwork.AdInfo adInfo, AdNetwork.AdError adError)
        {
            AdsEvents.AppOpen.OnAdShowFailedEvent(adInfo,adError);
        }

        private void AppOpenAd_OnAdShowed(AdNetwork.AdInfo adInfo)
        {
            _lastTimeShowAds = Time.realtimeSinceStartup;
            AdsEvents.AppOpen.OnAdShowedEvent(adInfo);
        }

        private AdNetwork.AdInfo _adInfo;
        private void AppOpenAd_OnAdLoaded(AdNetwork.AdInfo adInfo)
        {
            _adInfo = adInfo;
            AdsEvents.AppOpen.OnAdLoadedEvent(adInfo);
        }

        private void AppOpenAd_OnAdClosed(AdNetwork.AdInfo adInfo)
        {
            AdsEvents.AppOpen.OnAdClosedEvent(adInfo);
        }

        private void AppOpenAd_OnAdClicked(AdNetwork.AdInfo adInfo)
        {
            AdsEvents.AppOpen.OnAdClickedEvent(adInfo);
        }

        #endregion
    }
}

