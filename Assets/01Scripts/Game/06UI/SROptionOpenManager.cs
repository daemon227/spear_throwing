using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace DongA.UI
{
    public class SROption : MonoBehaviour
    {
        [SerializeField] private Button button;
        void Awake()
        {
            button.onClick.AddListener(OnButtonClick);
            DontDestroyOnLoad(gameObject);
        }

        void OnButtonClick()
        {
            if(RemoteConfig.Instance == null) return;
            if(SRDebug.Instance == null) return;
            if(!RemoteConfig.Instance.AdminTool) return;
            SRDebug.Instance.ShowDebugPanel();
        }
    }
}

