using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inwave.Production.Utils
{
#if DEBUG_ADS||DEBUG_ANALYTIC
    public class LogUtils
    {
        public static void Log(string tag, string content)
        {
            Debug.Log($"[{tag}] - {content}");
        }

        public static void LogError(string tag, string content)
        {
            Debug.LogError($"[{tag}] - {content}");
        }

        public static void LogWarning(string tag, string content)
        {
            Debug.LogWarning($"[{tag}] - {content}");
        }
    }
#endif
}

