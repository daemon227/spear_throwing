using System.Collections;
using System.Collections.Generic;
using DongA.Events;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace DongA.UI
{
    public class LosePopup : PopupBase
    {
        [SerializeField] private Button replayLevelButton;
        [SerializeField] private SkeletonGraphic skeletonGraphic;

        private void Awake()
        {
            if(replayLevelButton != null) replayLevelButton.onClick.AddListener(OnReplayLevelButtonClicked);
            InitializePopup();
        }
        void Start()
        {
            if (skeletonGraphic != null)
            {
                skeletonGraphic.Initialize(true);
                StartCoroutine(UpdateSpineUnscaled());
            }
            if (VibrationsManager.Instance != null) VibrationsManager.Instance.TriggerFailure();
        }
        private IEnumerator UpdateSpineUnscaled()
        {
            while (skeletonGraphic != null)
            {
                float dt = Time.unscaledDeltaTime;
                skeletonGraphic.AnimationState.Update(dt);
                skeletonGraphic.AnimationState.Apply(skeletonGraphic.Skeleton);
                skeletonGraphic.Skeleton.UpdateWorldTransform();
                yield return null;
            }
        }

        void OnReplayLevelButtonClicked()
        {
            // Debug.Log("Replay Level Button Clicked");
            AdsUtils.ShowInterstitial("popup_lose");
            AnalyticsNavigation.Button("popup_lose", ParameterName.replay);
            UIEvent.OnReplayLevelButtonClicked?.Invoke();
            UIEvent.OnClosePopupButtonClicked?.Invoke();
        }
        void OnCloseButtonClicked()
        {
            UIEvent.OnClosePopupButtonClicked?.Invoke();
        }

    }
}