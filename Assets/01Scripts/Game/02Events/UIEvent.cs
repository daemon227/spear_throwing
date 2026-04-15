using System;
using DongA.UI;
using UnityEngine;

namespace DongA.Events
{   
    public class UIEvent 
    {
        //UI events
        public static Action OnUpdateLevelText;
        //popup events
        public static Action<PopupType> OnShowPopup;
        //button events
        public static Action OnNextLevelButtonClicked;
        public static Action OnReplayLevelButtonClicked;
        public static Action OnClosePopupButtonClicked;
        public static Action<string> OnShowRewardPopup;
        public static Action<int> OnShowCoinRewardPopup;
        public static Action<int> OnCoinRewardClaimed;
        public static Action<bool> OnSetWinButtonsInteractable;
        public static Action<string> ShowNoticeString;
    }
}
