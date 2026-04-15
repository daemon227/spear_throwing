using System.Collections;
#if ADJUST
using com.adjust.sdk;
#endif
#if FIREBASE_SDK
using Firebase.Extensions;
#endif
using Inwave.Production.Ads;
using UnityEngine;

namespace Inwave.Production.Utils
{
    public class FTUENewUserInstallInit : MonoBehaviour
    {
        [SerializeField] private AdsManager _adsManager;

        private const string KEY_SAVE_PSEUDO_ID_USER = "User_Pseudo_Id";
        private const string FTUE_new_user_install = "FTUE_new_user_install";

        void Awake()
        {
            if (_adsManager != null)
            {
                _adsManager.gameObject.SetActive(false);
            }
        }
        IEnumerator Start()
        {
            if (_adsManager == null)
            {
                Debug.LogError("Assign AdsManager please !!!");
                yield return null;
            }
            else
            {
                yield return _Init();
            }
        }


        IEnumerator _Init()
        {
            while (!FirebaseSDK.Instance.Initialized)
            {
                yield return null;
            }

#if FIREBASE_SDK && ADJUST
            var adid = Adjust.getAdid();
            var idfa = Adjust.getIdfa();
#if UNITY_ANDROID
            var idfv = SystemInfo.deviceUniqueIdentifier;
#elif UNITY_IOS
            var idfv = UIDevice.identifierForVendor;
#endif
            var param = AnalyticHelper.CreateParam("adid", adid);
            param.Add("idfa", idfa);
            param.Add("idfv", idfv);

            AnalyticHelper.LogEvent(FTUE_new_user_install, param);

            string user_pseudo_id = null;
            if (PlayerPrefs.HasKey(KEY_SAVE_PSEUDO_ID_USER))
            {
                user_pseudo_id = PlayerPrefs.GetString(KEY_SAVE_PSEUDO_ID_USER);
            }
            else
            {
                Firebase.Analytics.FirebaseAnalytics.GetAnalyticsInstanceIdAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        user_pseudo_id = task.Result;
                        PlayerPrefs.SetString(KEY_SAVE_PSEUDO_ID_USER, user_pseudo_id);
                    }
                    else
                    {
                        Debug.LogError("Can not get Firebase pseudo ID.");
                    }
                });
            }
            AdjustEvent adjustEvent = new AdjustEvent(FTUE_new_user_install);
            adjustEvent.addPartnerParameter("user_pseudo_id", user_pseudo_id);
            Adjust.trackEvent(adjustEvent);

#if MAX_ADS
            MaxSdk.SetUserId($"FRB-{user_pseudo_id}");
#endif

#if IRONSOURCE_ADS
            IronSource.Agent.setUserId($"FRB-{user_pseudo_id}");
#endif

#endif
            yield return null;
            _adsManager.gameObject.SetActive(true);
        }
    }
}

