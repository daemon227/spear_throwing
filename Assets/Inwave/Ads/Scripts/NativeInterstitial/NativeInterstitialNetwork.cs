//#define DEBUG_ADS_INTERNAL
using System.Collections;
using System.Collections.Generic;
using Inwave.Production.Utils;
using UnityEngine;

namespace Inwave.Production.Ads
{
    public class NativeInterstitialNetwork : MonoBehaviour
    {
        protected static string LogTag = "NativeInterstitialNetwork";

        public virtual void Init(string adUnitId)
        {

        }
        public virtual void Show()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogWarning("Show");
#endif
        }

        public virtual void Hide()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogWarning("Hide");
#endif
        }

        public virtual void Load()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogWarning("Load");
#endif
        }

        public virtual void DestroyAd()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogWarning("DestroyAd");
#endif
        }

        protected virtual void Close()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogWarning("Close");
#endif
        }

        public virtual bool IsAvailable()
        {
            return false;
        }

#if DEBUG_ADS
        protected void LogWarning(string message)
        {
            LogUtils.LogWarning(LogTag,message);
        }

        protected void LogError(string message)
        {
            LogUtils.LogError(LogTag,message);
        }
#endif
    }
}

