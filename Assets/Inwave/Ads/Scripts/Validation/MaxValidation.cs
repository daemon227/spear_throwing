using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inwave.Production.Ads
{
    public class MaxValidation : MonoBehaviour
    {
#if MAX_ADS
        void Start()
        {
            MaxSdk.ShowMediationDebugger();
        }
#endif
    }
}

