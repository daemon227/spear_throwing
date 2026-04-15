#define DEBUG_ADS_INTERNAL
using System;
using System.Collections;
using Inwave.Production.Ads;
using Inwave.Production.Utils;
using UnityEngine;

namespace Inwave.Production.Test
{
    public class TestAds : AdsManagerInitializer
    {
        [SerializeField]
        private float _buttonWidth = 0.35f;
        [SerializeField]
        private float _buttonHeight = 0.06f;
        [SerializeField]
        private float _buttonSpaceY = 0.07f;
        [SerializeField]
        private float _y;

        private float y;

        protected override void Start()
        {
            base.Start();
            StartCoroutine(_ShowAppOpenAd());
        }

        IEnumerator _ShowAppOpenAd()
        {
            yield return new WaitForSeconds(0.1f);
            Ads.AdsManager.Instance.Show();
        }

        void OnGUI()
        {
            GUI.backgroundColor = Color.blue;
            GUI.skin.button.fontSize = (int)(0.035f * Screen.width);
            y = _y;
            CreateRewardButton();
            CreateInterstitialButtons();
            CreateBannerButtons();
            CreateAppOpenButtons();
            CreateMRecButtons();
            CreateBannerCollapsibleButtons();
            CreateFakeInterstitial();
        }


        void CreateRewardButton()
        {
            y += 0.1f;
            Rect showRewardedVideoButton = new Rect(0.10f * Screen.width, Screen.height*y, _buttonWidth*2 * Screen.width, _buttonHeight * Screen.height);
            if (GUI.Button(showRewardedVideoButton, "Show Rewarded Video"))
            {
                LogWarning("ShowRewardedVideoButtonClicked");

                Ads.AdsManager.Instance.ShowRewardAds((completed,location,adInfo) =>
                {
                    LogWarning($"Reward completed!: {completed} - Mediation: {adInfo.Mediation}");
                });
            }
        }

        void CreateInterstitialButtons()
        {
            y += _buttonSpaceY;
            Rect loadInterstitialButton = new Rect(0.10f * Screen.width, Screen.height*y, _buttonWidth * Screen.width, _buttonHeight * Screen.height);
            if (GUI.Button(loadInterstitialButton, "Load Interstitial"))
            {
                LogWarning("LoadInterstitialButtonClicked");
                Ads.AdsManager.Instance.LoadInterstitial();
            }

            Rect showInterstitialButton = new Rect(0.55f * Screen.width, Screen.height*y, _buttonWidth * Screen.width, _buttonHeight * Screen.height);
            if (GUI.Button(showInterstitialButton, "Show Interstitial"))
            {
                LogWarning("ShowInterstitialButtonClicked");
                if (Ads.AdsManager.Instance.IsInterstitialAvailable())
                {
                    Ads.AdsManager.Instance.RequestShowInterstitial(String.Empty);
                }
                else
                {
                    Ads.AdsManager.Instance.ShowFakeInterstitial();
                    LogWarning("Interstitial Ad is not available!");
                }
            }
        }

        void CreateBannerButtons()
        {
            y += _buttonSpaceY;
            Rect loadBannerButton = new Rect(0.10f * Screen.width, Screen.height*y, _buttonWidth * Screen.width, _buttonHeight * Screen.height);
            if (GUI.Button(loadBannerButton, "Load Banner"))
            {
                LogWarning("loadBannerButtonClicked");
                Ads.AdsManager.Instance.DelayLoadBanner();
            }
            Rect hideBannerButton = new Rect(0.55f * Screen.width, Screen.height * y, _buttonWidth * Screen.width, _buttonHeight * Screen.height);
            if (GUI.Button(hideBannerButton, "Hide Banner"))
            {
                LogWarning("hideBannerButtonClicked");
                Ads.AdsManager.Instance.HideBanner();
            }


            y += _buttonSpaceY;

            Rect showBannerButton = new Rect(0.10f * Screen.width, Screen.height * y, _buttonWidth * Screen.width, _buttonHeight * Screen.height);
            if (GUI.Button(showBannerButton, "Show Banner"))
            {
                LogWarning("showBannerButtonClicked");
                Ads.AdsManager.Instance.ShowBanner();
            }
            Rect destroyBannerButton = new Rect(0.55f * Screen.width, Screen.height * y, _buttonWidth * Screen.width, _buttonHeight * Screen.height);
            if (GUI.Button(destroyBannerButton, "Destroy Banner"))
            {
                LogWarning("destroyBannerButtonClicked");
                Ads.AdsManager.Instance.DestroyBanner();
            }
        }

        void CreateAppOpenButtons()
        {
            y += _buttonSpaceY;
            Rect rect = new Rect(0.1f * Screen.width, Screen.height*y, _buttonWidth * Screen.width, _buttonHeight * Screen.height);
 
            string text = string.Empty;
            if (Ads.AdsManager.Instance.EnableAppOpenAd)
            {
                text = "Disable AppOpen";
            }
            else
            {
                text = "Enable AppOpen";
            }
            if (GUI.Button(rect, text))
            {
                Ads.AdsManager.Instance.EnableAppOpenAd = !Ads.AdsManager.Instance.EnableAppOpenAd;
            }
        }

        void CreateMRecButtons()
        {
            y+= _buttonSpaceY;
            Rect loadBannerButton = new Rect(0.10f * Screen.width, Screen.height *y, _buttonWidth * Screen.width, _buttonHeight * Screen.height);
            if (GUI.Button(loadBannerButton, "Load MRec"))
            {
                LogWarning("loadMRecButtonClicked");
                Ads.AdsManager.Instance.CreateMRecAd();
            }


            Rect hideBannerButton = new Rect(0.55f * Screen.width, Screen.height * y, _buttonWidth * Screen.width, _buttonHeight * Screen.height);
            if (GUI.Button(hideBannerButton, "Hide MRec"))
            {
                LogWarning("hideMRecButtonClicked");
                Ads.AdsManager.Instance.HideMRecAd();
            }

            y += _buttonSpaceY;

            Rect showBannerButton = new Rect(0.10f * Screen.width, Screen.height * y, _buttonWidth * Screen.width, _buttonHeight * Screen.height);
            if (GUI.Button(showBannerButton, "Show MRec Ad"))
            {
                LogWarning("showMRecButtonClicked");
                Ads.AdsManager.Instance.ShowMRecAd();
            }
            Rect destroyBannerButton = new Rect(0.55f * Screen.width, Screen.height * y, _buttonWidth * Screen.width, _buttonHeight * Screen.height);
            if (GUI.Button(destroyBannerButton, "Destroy MRec"))
            {
                LogWarning("destroyMRecButtonClicked");
                Ads.AdsManager.Instance.DestroyMRecAd();
            }
        }

        void CreateBannerCollapsibleButtons()
        {
            y += _buttonSpaceY;
            Rect loadBannerButton = new Rect(0.10f * Screen.width, Screen.height * y, _buttonWidth * Screen.width, _buttonHeight * Screen.height);
            if (GUI.Button(loadBannerButton, "Load Banner Co Bottom"))
            {
                Ads.AdsManager.Instance.LoadBannerCollapsible(BannerCollapsibleNetwork.BannerCollapsiblePosition.BOTTOM);
            }
            Rect hideBannerButton = new Rect(0.55f * Screen.width, Screen.height * y, _buttonWidth * Screen.width, _buttonHeight * Screen.height);
            if (GUI.Button(hideBannerButton, "Load Banner Co Top"))
            {
                Ads.AdsManager.Instance.LoadBannerCollapsible(BannerCollapsibleNetwork.BannerCollapsiblePosition.TOP);
            }


            y += _buttonSpaceY;

            Rect showBannerButton = new Rect(0.10f * Screen.width, Screen.height * y, _buttonWidth * Screen.width, _buttonHeight * Screen.height);
            if (GUI.Button(showBannerButton, "Show Banner Co Top"))
            {
                ShowCollapsible(BannerCollapsibleNetwork.BannerCollapsiblePosition.TOP);
            }
            Rect destroyBannerButton = new Rect(0.55f * Screen.width, Screen.height * y, _buttonWidth * Screen.width, _buttonHeight * Screen.height);
            if (GUI.Button(destroyBannerButton, "Show Banner Co Bottom"))
            {
                //LogWarning("destroyBannerCollapsibleButtonClicked");
                ShowCollapsible(BannerCollapsibleNetwork.BannerCollapsiblePosition.BOTTOM);
            }
        }

        void CreateFakeInterstitial()
        {
            y += _buttonSpaceY;
            Rect loadInterstitialButton = new Rect(0.10f * Screen.width, Screen.height * y, _buttonWidth * Screen.width, _buttonHeight * Screen.height);
            if (GUI.Button(loadInterstitialButton, "Load Fake Inters"))
            {
                LogWarning("LoadFakeIntersButtonClicked");
                Ads.AdsManager.Instance.LoadFakeInterstitial();
            }

            Rect showInterstitialButton = new Rect(0.55f * Screen.width, Screen.height * y, _buttonWidth * Screen.width, _buttonHeight * Screen.height);
            if (GUI.Button(showInterstitialButton, "Show Fake Inters"))
            {
                LogWarning("ShowFakeIntersButtonClicked");
                if (Ads.AdsManager.Instance.IsFakeInterstitialAvailable())
                {
                    Ads.AdsManager.Instance.ShowFakeInterstitial();
                }
                else
                {

                    LogWarning("Fake Inters Ad is not available!");
                }
            }
        }

        private void ShowCollapsible(BannerCollapsibleNetwork.BannerCollapsiblePosition position)
        {
            if (AdsManager.Instance.IsBannerCollapsibleAvailable)
            {
                LogWarning($"Show Banner Collapsible");
                AdsManager.Instance.ShowBannerCollapsible(position);
            }
            else
            {
                LogWarning("Show Replacement Banner");
                AdNetwork.BannerPosition bannerPosition = AdNetwork.BannerPosition.TOP;
                if (position == BannerCollapsibleNetwork.BannerCollapsiblePosition.BOTTOM)
                {
                    bannerPosition = AdNetwork.BannerPosition.BOTTOM;
                }

                AdsManager.Instance.BannerPosition = bannerPosition;
                AdsManager.Instance.ShowBanner();
            }
        }


        #region Utils
        void LogWarning(string content)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning("TestAds", content);
#endif
        }
        #endregion

    }
}

