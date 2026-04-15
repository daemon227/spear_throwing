#define TEST_INTERNAL
using System;
using System.Collections;
using System.Collections.Generic;
using Inwave.Production.Utils;
using UnityEngine;

namespace Inwave.Production.Ads
{
    //AdsManager Fake Interstitial
    public partial class AdsManager:MonoBehaviour
    {
#if DEBUG_ADS
        private const string LogTagAdsManagerFakeInters = "AdsManager Fake Inters";
#endif
        [Header("+Fake Interstitial++++++")]
        [SerializeField] private string _fakeIntersAdUnitIdAndroid = "YOUR_AD_UNIT_ID";
        [SerializeField] private string _fakeIntersAdUnitIdIOS = "YOUR_AD_UNIT_ID";
        [SerializeField] private bool _enableFakeInterstitial = false;
#if TEST_INTERNAL
        [SerializeField] private bool _useTestId = false;
#endif
        private NativeInterstitialNetwork _nativeInterstitialNetwork;

        public bool EnableFakeInterstitial
        {
            get => _enableFakeInterstitial;
            set => _enableFakeInterstitial = value;
        }

        private void InitFakeInterstitial()
        {
            if (_enableFakeInterstitial)
            {
                var fakeInters = Resources.Load<NativeInterstitialNetwork>("AdPrefabs/AdmobNativeInterstitial");
                if (fakeInters != null)
                {
                    _nativeInterstitialNetwork = Instantiate(fakeInters);
                    DontDestroyOnLoad(_nativeInterstitialNetwork.gameObject);
                    string adUnitId = String.Empty;
#if UNITY_ANDROID
                    adUnitId = _fakeIntersAdUnitIdAndroid;
#elif UNITY_IOS
                    adUnitId = _fakeIntersAdUnitIdIOS;
#endif
#if TEST_INTERNAL
                    if (_useTestId)
                    {
                        adUnitId = "ca-app-pub-3940256099942544/2247696110";
                    }
#endif
                    _nativeInterstitialNetwork.Init(adUnitId);
                    AdsEvents.FakeInterstitial.OnAdContentOpened += FakeInterstitial_OnAdContentOpened;
                    AdsEvents.FakeInterstitial.OnAdContentClosed += FakeInterstitial_OnAdContentClosed;
                    LoadFakeInterstitial();
                }
#if DEBUG_ADS
                else
                {
                    LogUtils.LogError(LogTagAdsManagerFakeInters, "_nativeInterstitialNetwork is null!");
                }
#endif
            }
        }

        private void FakeInterstitial_OnAdContentClosed(AdNetwork.AdInfo obj)
        {
            StartCoroutine(_Delay());
            IEnumerator _Delay()
            {
                yield return null;
                SetCanShowAppOpenAdd(true);
            }
        }

        private void FakeInterstitial_OnAdContentOpened(AdNetwork.AdInfo obj)
        {
            SetCanShowAppOpenAdd(false);
        }

        public void ShowFakeInterstitial()
        {
            if (_enableFakeInterstitial)
            {
                _nativeInterstitialNetwork?.Show();
            }
        }

        public void LoadFakeInterstitial()
        {
            if (_enableFakeInterstitial)
            {
                _nativeInterstitialNetwork?.Load();
            }
        }

        public void HideFakeInterstitial()
        {
            if (_enableFakeInterstitial)
            {
                _nativeInterstitialNetwork?.Hide();
            }
        }

        public void DestroyFakeInterstitial()
        {
            if (_enableFakeInterstitial)
            {
                _nativeInterstitialNetwork?.DestroyAd();
            }
        }

        public bool IsFakeInterstitialAvailable()
        {
            if (_enableBannerCollapsible && _nativeInterstitialNetwork != null)
            {
                return _nativeInterstitialNetwork.IsAvailable();
            }
            return false;
        }
    }
}

