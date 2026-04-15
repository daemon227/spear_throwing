using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inwave.Production.Ads
{
    //public interface IPlacement
    //{
    //    string GetValue(AdsManager manager);
    //}
    public enum AdDelayOption
    {
        BeforeFirstFS,
        BetweenFS,
        BetweenRVAndFS,
        InterstitialDelayConfig
    }

    public enum AdErrorType
    {
        FsShow,
        FsCaching,
        RwShow,
        RwCaching
    }
}
