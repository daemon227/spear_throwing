using System.Collections;
using System.Collections.Generic;
using Inwave.Production.Ads;
using Inwave.Production.Utils;
using UnityEngine;
#if FIREBASE_SDK
#endif
namespace Inwave.Production.Test
{
    public class TestMediationDataSegment : MonoBehaviour
    {
        void Start()
        {
#if FIREBASE_SDK
            RemoteConfig.Instance.OnEndLoad += Instance_OnEndLoad;
#endif
            AdsEvents.OnSetCustomSegmentData += Instance_OnSetCustomSegmentData;
        }

        private void Instance_OnSetCustomSegmentData(string key, string value)
        {
#if DEBUG_ADS||DEBUG_ANALYTIC
            LogUtils.LogWarning("TestMediationDataSegment",$"Key: {key} - Value: {value}");
#endif
#if FIREBASE_SDK
            AnalyticHelper.LogEvent(key);
#endif
        }

#if FIREBASE_SDK
        private void Instance_OnEndLoad()
        {
            List<string> segments = new List<string>();
            segments.Add(RemoteConfig.Instance.MediationDataSegment);
            AdsManager.Instance.SetCustomSegmentData(segments);

        }
#endif
    }
}

