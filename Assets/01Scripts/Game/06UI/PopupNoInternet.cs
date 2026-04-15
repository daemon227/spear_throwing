using System.Collections;
using System.Collections.Generic;
using DongA.Events;
using UnityEngine;
using UnityEngine.UI;

namespace DongA.UI
{
    public class PopupNoInternet : PopupBase
    {
        [SerializeField] private Button closeButton;
        void Start()
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
        void OnCloseButtonClicked()
        {
            UIEvent.OnClosePopupButtonClicked?.Invoke();
        }
    }
}
