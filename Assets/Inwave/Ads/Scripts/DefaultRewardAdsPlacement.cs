using Inwave.Production.Ads;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inwave.Production.Ads
{
    public class DefaultRewardAdsPlacement
    {
        public string GetValue(AdsManager adsManager)
        {
            var type = adsManager.RewardedType;
            switch (type)
            {
                case VIDEOREWARD.revive:
                    return "reborn";

                case VIDEOREWARD.currency_x2:
                    return "double";

                case VIDEOREWARD.CHALLENGE:
                    return "challenge_home";
                case VIDEOREWARD.SKIPABLE:
                    return "skip";
                case VIDEOREWARD.LUCKY_SPIN:
                    return "lucky_spin";
                case VIDEOREWARD.item:
                    return "item";
                default:
                    break;
            }

            return null;
        }
    }
}

