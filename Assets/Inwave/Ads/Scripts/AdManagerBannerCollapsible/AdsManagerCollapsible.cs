//#define DEBUG_ADS_INTERNAL
using System.Collections;
using System.Collections.Generic;
using Inwave.Production.Utils;
using UnityEngine;
using static Inwave.Production.Ads.BannerCollapsibleNetwork;

namespace Inwave.Production.Ads
{
    //Banner Collapsible
    public partial class AdsManager : MonoBehaviour
    {
#if DEBUG_ADS
        private const string LogTagAdsManagerBannerCollapsible = "AdsManager Banner Collapsible";
#endif
        [Header("+Banner Collapsible------")]
        [SerializeField]
        private BannerCollapsibleNetwork _bannerCollapsible;
        [SerializeField]
        private bool _enableBannerCollapsible = true;

        //[SerializeField]
        private BannerCollapsiblePosition _bannerCollapsiblePositionCollapsed = BannerCollapsiblePosition.BOTTOM;

        [SerializeField]
        //'-1' for not switching
        protected float _timeToSwitchToReplacement = 10;

        [SerializeField] private bool _useCustomReplacementPosition = false;

        [SerializeField] private BannerCollapsiblePosition _replacementPosition = BannerCollapsiblePosition.BOTTOM;

        [SerializeField] private bool _destroyCollapAfterClose = false;

        private bool _allowBannerCollapsible;

        public bool EnableBannerCollapsible
        {
            get=>_enableBannerCollapsible;
            set
            {
                _enableBannerCollapsible = value;
                _allowBannerCollapsible = value;
            }
        }

        public BannerCollapsiblePosition BannerCollapsiblePositionCollapsed
        {
            get => _bannerCollapsiblePositionCollapsed;
            set => _bannerCollapsiblePositionCollapsed = value;
        }

        private void InitBannerCollapsible()
        {
            if (!_enableBannerCollapsible || _bannerCollapsible==null)
            {
                return;
            }
#if DEBUG_ADS &&DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTagAdsManagerBannerCollapsible, "InitBanner");
#endif
            if (Application.isEditor)
            {
                _allowBannerCollapsible = false;
            }
            _allowBannerCollapsible = true;
            _bannerCollapsible.Position = _bannerCollapsiblePositionCollapsed;
            _bannerCollapsible.Initialize();

            AdsEvents.BannerCollapsible.OnNeedReplacement += BannerCollapsible_OnNeedReplacement;
            if (_timeToSwitchToReplacement > 0)
            {
                AdsEvents.BannerCollapsible.OnAdScreenPresented += BannerCollapsible_OnAdScreenPresented;
                AdsEvents.BannerCollapsible.OnAdScreenDismissed+= BannerCollapsible_OnAdScreenDismissed;
                AdsEvents.BannerCollapsible.OnAdImpressionRecorded += BannerCollapsible_OnAdImpressionRecorded;
            }
            
        }

        //bool _isNormalBannerFullscreen = false;
        private bool _isCollapBannerShowing = false;
        private void BannerCollapsible_OnAdScreenPresented(AdNetwork.AdInfo adInfo, bool isCollapsible)
        {
            if (isCollapsible)
            {
                _isCollapBannerShowing = true;
            }
        }

        private void BannerCollapsible_OnAdImpressionRecorded(AdNetwork.AdInfo adInfo, bool isCollapsible)
        {
            //wait for checking Collapsible or Normal
            StartCoroutine(_Wait());
            IEnumerator _Wait()
            {
                while (_isCollapBannerShowing)
                {
                    yield return null;
                }
                yield return new WaitForSeconds(_timeToSwitchToReplacement);
                SwitchToReplacement();
            }
        }

        private void BannerCollapsible_OnAdScreenDismissed(AdNetwork.AdInfo adInfo)
        {
            _isCollapBannerShowing = false;
        }

        private void SwitchToReplacement()
        {
            if (_destroyCollapAfterClose)
            {
                DestroyBannerCollapsible();
            }
            else
            {
                HideBannerCollapsible();
            }

            LoadBannerCollapsible(_bannerCollapsiblePositionCollapsed);

            if (_useCustomReplacementPosition)
            {
                var bannerPosition = CollapseBannerPositionToNormalBannerPosition(_replacementPosition);
                AdsManager.Instance.BannerPosition = bannerPosition;
            }
            else
            {
                var bannerPosition = CollapseBannerPositionToNormalBannerPosition(_bannerCollapsiblePositionCollapsed);
                AdsManager.Instance.BannerPosition = bannerPosition;
            }


#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogError(LogTagAdsManagerBannerCollapsible,
                $"Switch to replacement Banner: {_timeToSwitchToReplacement}s");
#endif
            ShowBanner();
        }

        private AdNetwork.BannerPosition CollapseBannerPositionToNormalBannerPosition(BannerCollapsiblePosition position)
        {
            AdNetwork.BannerPosition bannerPosition = AdNetwork.BannerPosition.TOP;
            if (position == BannerCollapsiblePosition.BOTTOM)
            {
                bannerPosition = AdNetwork.BannerPosition.BOTTOM;
            }

            return bannerPosition;
        }

        private bool _useReplacement = false;
        private void BannerCollapsible_OnNeedReplacement(BannerCollapsiblePosition position)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTagAdsManagerBannerCollapsible, "OnNeedReplacement");
#endif
            _useReplacement = true;

            var bannerPosition = CollapseBannerPositionToNormalBannerPosition(position);
            AdsManager.Instance.BannerPosition = bannerPosition;
            ShowBanner();
        }

        private void OnDestroy()
        {
            AdsEvents.BannerCollapsible.OnNeedReplacement -= BannerCollapsible_OnNeedReplacement;
            AdsEvents.BannerCollapsible.OnAdImpressionRecorded -= BannerCollapsible_OnAdImpressionRecorded;
            AdsEvents.BannerCollapsible.OnAdScreenDismissed -= BannerCollapsible_OnAdScreenDismissed;
        }

        public void LoadBannerCollapsible(BannerCollapsiblePosition position)
        {
            if (_allowBannerCollapsible)
            {
#if DEBUG_ADS &&DEBUG_ADS_INTERNAL
                LogUtils.LogWarning(LogTagAdsManagerBannerCollapsible, "LoadBannerCollapsible");
#endif
                _bannerCollapsiblePositionCollapsed = position;

                _bannerCollapsible?.Load(position);
            }
        }

        public void HideBannerCollapsible()
        {
            if (_bannerCollapsible != null)
            {
#if DEBUG_ADS &&DEBUG_ADS_INTERNAL
                LogUtils.LogWarning(LogTagAdsManagerBannerCollapsible, "HideBannerCollapsible");
#endif
                _bannerCollapsible.Hide();
            }
        }

        public void DestroyBannerCollapsible()
        {
            if (_bannerCollapsible != null)
            {
#if DEBUG_ADS &&DEBUG_ADS_INTERNAL
                LogUtils.LogWarning(LogTagAdsManagerBannerCollapsible, "DestroyBannerCollapsible");
#endif
                _bannerCollapsible.DestroyAd();
            }
        }

        public void ShowBannerCollapsible(BannerCollapsiblePosition position)
        {
            if (_useReplacement)
            {
                _useReplacement = false;
                HideBanner();
            }
            if (_enableBanner && _bannerCollapsible != null)
            {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                LogUtils.LogWarning(LogTagAdsManagerBannerCollapsible, "ShowBannerCollapsible");
#endif

                _bannerCollapsiblePositionCollapsed = position;
                _bannerCollapsible.Show(position);
            }
        }

        public bool IsBannerCollapsibleAvailable => _bannerCollapsible != null && _bannerCollapsible.IsAvailable;

        public BannerCollapsibleType BannerCollapsibleType
        {
            get
            {
                if (_bannerCollapsible != null)
                {
                    if (_bannerCollapsible.IsCollapsible)
                    {
                        return BannerCollapsibleType.Collapsible;
                    }
                    else
                    {
                        return BannerCollapsibleType.Normal;
                    }
                }
                return BannerCollapsibleType.None;
            }
        }
    }
}

