using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using AppsFlyerSDK;
#if MAX_ADS
using Inwave.Production.Ads;
#endif

public class AppsFlyerManager : AppsFlyer, IAppsFlyerConversionData {
    #region properties

    public static AppsFlyerManager Instance;

    [Header("General:")]
    public string appsFlyerKey;
#if ADJUST
    public bool enableAdjustMapping = true;
#endif    
    public bool isDebug = false;

    [Header("IOS Settings:")]
    public string iosAppId;

    public bool requestATT = true;

    #endregion

    #region Unity functions

    private void Awake() {
        DontDestroyOnLoad(this.gameObject);
        Instance = this;
    }

    private void Start() {
#if FIREBASE_SDK
        FirebaseSDK.Instance.OnFirebaseReady += OnFirebaseReady;
#else
        if (isDebug) {
            Debug.Log("[AppsFlyerManager] Lưu ý set callback từ firebase sau khi init thành công. VD: AppsFlyerManager.Instance.OnFirebaseReady()");
        }
#endif
#if MAX_ADS
        AdsEvents.OnAdImpressionSuccess += OnAdImpressionSuccess;
#else
        if (isDebug) {
            Debug.Log("[AppsFlyerManager] Lưu ý set callback từ Max mediation khi có impression ad thành công.VD: AppsFlyerManager.Instance.OnAdImpressionSuccess(adInfo)");
        }
#endif

        // For detailed logging
        if (isDebug) {
            AppsFlyer.setIsDebug(true);
        }

        //Start Appsflyer SDK
        var appId = Application.platform == RuntimePlatform.IPhonePlayer ? iosAppId : Application.identifier;
        AppsFlyer.initSDK(appsFlyerKey, appId, this);

        if (isDebug && (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appsFlyerKey))) {
            Debug.LogError("[AppsFlyerManager] Kiểm tra lại các giá trị iosAppId/appsFlyerKey/appId/");
        }

#if UNITY_IOS && !UNITY_EDITOR
        // 2024/04/29 Hotfix for DnI tracking
        AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(60);
#endif
        // Purchase connector implementation 
        InitPurchaseConnector(this);

        // AdRevenue connector implementation 
        AppsFlyerAdRevenue.start();
        if (isDebug) {
            AppsFlyerAdRevenue.setIsDebug(true);
        }

        AppsFlyer.startSDK();
    }

    #endregion

    #region Conversion events

    private void InitPurchaseConnector(MonoBehaviour appsflyerMonoBehaviour = null) {
        AppsFlyerPurchaseConnector.init(appsflyerMonoBehaviour, Store.GOOGLE); // appsflyerMonoBehaviour is us
        AppsFlyerPurchaseConnector.setIsSandbox(false);
        AppsFlyerPurchaseConnector.setAutoLogPurchaseRevenue(
            AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsAutoRenewableSubscriptions,
            AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsInAppPurchases);
        AppsFlyerPurchaseConnector.setPurchaseRevenueValidationListeners(true);
        AppsFlyerPurchaseConnector.build();
        AppsFlyerPurchaseConnector.startObservingTransactions();
    }

    public void OnFirebaseReady() {
        // Init FirebaseMessaging
        Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
        MappingUserIdTracking.Instance.StartTracking();
    }

#if MAX_ADS
    private void OnAdImpressionSuccess(AdNetwork.AdImpressionData revenueData) {

        Dictionary<string, string> dic = new Dictionary<string, string>();
        dic.Add(AFAdRevenueEvent.AD_UNIT, revenueData.UnitName);
        dic.Add(AFAdRevenueEvent.AD_TYPE, revenueData.Format);
        if (!string.IsNullOrEmpty(revenueData.Placement)) {
            dic.Add(AFAdRevenueEvent.PLACEMENT, revenueData.Placement);
        }

        AFAdRevenueData logRevenue = new(revenueData.Source, MediationNetwork.ApplovinMax, revenueData.Currency,
            revenueData.Revenue);
        AppsFlyer.logAdRevenue(logRevenue, dic);
        if (isDebug) {
            Debug.Log("[AppsFlyerManager] TrackRevenue " + revenueData.Source + " " + revenueData.Revenue + " " +
                      revenueData.Currency + " " + AFMiniJSON.Json.Serialize(dic));
        }
    }
#else
    private void OnAdImpressionSuccess(MaxSdkBase.AdInfo revenueData) {

        Dictionary<string, string> dic = new Dictionary<string, string>();
        dic.Add(AFAdRevenueEvent.AD_UNIT, revenueData.AdUnitIdentifier);
        dic.Add(AFAdRevenueEvent.AD_TYPE, revenueData.AdFormat);
        if (!string.IsNullOrEmpty(revenueData.Placement)) {
            dic.Add(AFAdRevenueEvent.PLACEMENT, revenueData.Placement);
        }

        AFAdRevenueData logRevenue = new(revenueData.NetworkPlacement, MediationNetwork.ApplovinMax, "USD",
            revenueData.Revenue);
        AppsFlyer.logAdRevenue(logRevenue, dic);
        if (isDebug) {
            Debug.Log("[AppsFlyerManager] TrackRevenue " + revenueData.NetworkName + " " + revenueData.Revenue + " " +
                      "USD" + " " + AFMiniJSON.Json.Serialize(dic));
        }
    }
#endif

    public void onConversionDataSuccess(string conversionData) {
        if (isDebug) {
            Debug.Log("[AppsFlyerManager] onConversionDataSuccess " + conversionData);
        }
    }

    public void onConversionDataFail(string error) {
        if (isDebug) {
            Debug.Log("[AppsFlyerManager] onConversionDataFail" + error);
        }
    }

    public void onAppOpenAttribution(string attributionData) {
        if (isDebug) {
            Debug.Log("[AppsFlyerManager] onAppOpenAttribution" + attributionData);
        }
    }

    public void onAppOpenAttributionFailure(string error) {
        if (isDebug) {
            Debug.Log("[AppsFlyerManager] onAppOpenAttributionFailure" + error);
        }
    }

    #endregion

    #region Uninstall measurement Tracking

    private bool _tokenSent;
#if UNITY_IOS
    private void Update() {
        if (!_tokenSent && Application.platform == RuntimePlatform.IPhonePlayer && Time.frameCount % 30 == 0) {
            byte[] token = null;

            token = UnityEngine.iOS.NotificationServices.deviceToken;
            if (token != null) {
                registerUninstall(token); //IOS
                _tokenSent = true;
            }
            if (isDebug) {
                Debug.Log("[AppsFlyerManager] Uninstall measurement Tracking Token " + token);
            }

        }
    }
#endif
    public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token) {
#if UNITY_ANDROID
        AppsFlyer.updateServerUninstallToken(token.Token);
        if (isDebug) {
            Debug.Log("[AppsFlyerManager] Uninstall measurement Tracking Token " + token.Token);
        }
#endif
    }

    #endregion
}