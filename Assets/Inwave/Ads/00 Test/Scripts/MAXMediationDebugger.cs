using UnityEngine;

namespace Inwave.Production.Test
{
    public class MAXMediationDebugger : MonoBehaviour
    {
        [SerializeField] private bool _dontDestroyOnLoad = true;
        void Awake()
        {
            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
#if MAX_ADS
        void OnGUI()
        {
            CreateShowDebuggerButton();
        }

        void CreateShowDebuggerButton()
        {
            Rect showMAXMediationDebugger = new Rect(0.10f * Screen.width, 0.05f * Screen.height, 0.60f * Screen.width, 0.05f * Screen.height);
            if (GUI.Button(showMAXMediationDebugger, "Show MAX Debugger"))
            {
                MaxSdk.ShowMediationDebugger();
            }
        }
#endif
    }
}

