using DongA.Events;
using DongA.GameDatas;
using DongA.Obstacles;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace DongA.UI
{
    public class ProgressBarAndReward : MonoBehaviour
    {
        [SerializeField] private Image fillBar;
        [SerializeField] private TextMeshProUGUI currentLevel;
        [SerializeField] private List<RewardSlotUI> rewardSlotUIs;
        [SerializeField] private float fillDuration = 0.6f;

        [Header("Milestone Coin Reward")]
        [SerializeField] private CoinAnim coinAnim;
        [SerializeField] private Transform coinUI;
        [SerializeField] private int milestone30Coin = 50;
        [SerializeField] private int milestone60Coin = 100;

        [Header("Level 100 Spear Reward")]
        [SerializeField] private AllSpearSkinDataSO allSpearSkinDataSO;
        [SerializeField] private int fallbackCoin = 200;
        [SerializeField] private float resetDelay = 1.2f;

        void Start()
        {
            if(RemoteConfig.Instance!= null)
            {
                milestone30Coin = RemoteConfig.Instance.Coin30;
                milestone60Coin = RemoteConfig.Instance.Coin60;
                fallbackCoin = RemoteConfig.Instance.Coin100;
            }
            UpdateProgressBar();
        }

        void OnEnable()
        {
            UIEvent.OnCoinRewardClaimed += OnCoinRewardClaimed;
        }

        void OnDisable()
        {
            UIEvent.OnCoinRewardClaimed -= OnCoinRewardClaimed;
        }

        private void OnCoinRewardClaimed(int coinAmount)
        {
            // GamePrefs.Coin += coinAmount;
            if (coinAnim != null && coinUI != null && rewardSlotUIs.Count > 2 && rewardSlotUIs[2] != null)
            {
                coinAnim.PlayFromWorldPos(rewardSlotUIs[2].transform.position, coinUI, coinAmount);
                rewardSlotUIs[2].SetupSlotState(true);
            }
            AnalyticsResouces.InCome("progress_bar", "win_level", ResourceType.Coin, ParameterValue.coin, coinAmount);
        }
        // private void OnEnable()
        // {
        //     StartCoroutine(UpdateProgressBarNextFrame());
        // }

        // private IEnumerator UpdateProgressBarNextFrame()
        // {
        //     yield return null;
        //     UpdateProgressBar();
        // }

        private void UpdateProgressBar()
        {
            int level = GamePrefs.Level + 1;
            currentLevel.text = level.ToString();

            int progressCycleNumber = level / 100;
            float displayProgress = level - (progressCycleNumber * 100);
            bool isCycleComplete = level > 0 && displayProgress == 0;

            // cycle đang hiển thị: nếu vừa hoàn thành thì là cycle trước
            int displayCycle = isCycleComplete ? progressCycleNumber - 1 : progressCycleNumber;

            float normalizedProgress = isCycleComplete ? 1f : displayProgress / 100f;
            UpdateRewardSlotUI(normalizedProgress, progressCycleNumber);
            UpdateRewardSlotUIText(displayCycle);

            fillBar.DOKill();
            fillBar.fillAmount = 0;
            fillBar.DOFillAmount(normalizedProgress, fillDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (isCycleComplete)
                        GiveRewardAndReset(level);
                    else
                        GiveMilestoneCoinReward(normalizedProgress, progressCycleNumber);
                });
        }

        private void GiveMilestoneCoinReward(float progress, int cycleNumber)
        {
            if (coinAnim == null) return;
            // var coinTarget = UIManager.Instance != null ? UIManager.Instance.coinBar.transform : null;
            // if (coinTarget == null) return;

            int milestone30LastCycle = PlayerPrefs.GetInt(KeyPrefs.Milestone30LastCycle, -1);
            int milestone60LastCycle = PlayerPrefs.GetInt(KeyPrefs.Milestone60LastCycle, -1);

            if (progress == 0.3f && rewardSlotUIs.Count > 0 && rewardSlotUIs[0] != null && cycleNumber > milestone30LastCycle)
            {
                PlayerPrefs.SetInt(KeyPrefs.Milestone30LastCycle, cycleNumber);
                PlayerPrefs.Save();
                var slot0 = rewardSlotUIs[0];
                int coin30 = milestone30Coin;
                UIEvent.OnSetWinButtonsInteractable?.Invoke(false);
                slot0.PlayMilestoneAnim(() =>
                {
                    GamePrefs.Coin += coin30;
                    coinAnim.PlayFromWorldPos(slot0.transform.position, coinUI, coin30);
                    slot0.SetupSlotState(true);
                    AnalyticsResouces.InCome("progress_bar", "win_level", ResourceType.Coin, ParameterValue.coin, coin30);
                    UIEvent.OnSetWinButtonsInteractable?.Invoke(true);
                });
            }

            if (progress == 0.6f && rewardSlotUIs.Count > 1 && rewardSlotUIs[1] != null && cycleNumber > milestone60LastCycle)
            {
                PlayerPrefs.SetInt(KeyPrefs.Milestone60LastCycle, cycleNumber);
                PlayerPrefs.Save();
                var slot1 = rewardSlotUIs[1];
                int coin60 = milestone60Coin;
                UIEvent.OnSetWinButtonsInteractable?.Invoke(false);
                slot1.PlayMilestoneAnim(() =>
                {
                    GamePrefs.Coin += coin60;
                    coinAnim.PlayFromWorldPos(slot1.transform.position, coinUI, coin60);
                    slot1.SetupSlotState(true);
                    AnalyticsResouces.InCome("progress_bar", "win_level", ResourceType.Coin, ParameterValue.coin, coin60);
                    UIEvent.OnSetWinButtonsInteractable?.Invoke(true);
                });
            }
        }

        private void UpdateRewardSlotUI(float progress, int cycleNumber)
        {
            int milestone30LastCycle = PlayerPrefs.GetInt(KeyPrefs.Milestone30LastCycle, -1);
            int milestone60LastCycle = PlayerPrefs.GetInt(KeyPrefs.Milestone60LastCycle, -1);

            if (rewardSlotUIs.Count > 0 && rewardSlotUIs[0] != null)
            {
                bool claimed30 = progress > 0.3f || (progress == 0.3f && cycleNumber <= milestone30LastCycle);
                rewardSlotUIs[0].SetupSlotState(claimed30);
            }
            if (rewardSlotUIs.Count > 1 && rewardSlotUIs[1] != null)
            {
                bool claimed60 = progress > 0.6f || (progress == 0.6f && cycleNumber <= milestone60LastCycle);
                rewardSlotUIs[1].SetupSlotState(claimed60);
            }
            if (rewardSlotUIs.Count > 2 && rewardSlotUIs[2] != null)
                rewardSlotUIs[2].SetupSlotState(progress >= 1f);
        }
        private void UpdateRewardSlotUIText(int cycleNumber)
        {
            int base100 = cycleNumber * 100;
            if (rewardSlotUIs.Count > 0 && rewardSlotUIs[0] != null)
            {
                rewardSlotUIs[0].LevelRecive.text = "Lv." + (30 + base100);
                rewardSlotUIs[0].SetCoinAmountText("x "+ milestone30Coin.ToString());
            }
            if (rewardSlotUIs.Count > 1 && rewardSlotUIs[1] != null)
            {
                rewardSlotUIs[1].LevelRecive.text = "Lv." + (60 + base100);
                rewardSlotUIs[1].SetCoinAmountText("x "+ milestone60Coin.ToString());
            }
            if (rewardSlotUIs.Count > 2 && rewardSlotUIs[2] != null)
            {
                rewardSlotUIs[2].LevelRecive.text = "Lv." + (100 + base100);
                rewardSlotUIs[2].SetCoinAmountText("? / " + fallbackCoin);
            }
        }

        private void GiveRewardAndReset(int currentLevel)
        {
            int currentCycle = currentLevel / 100;
            int lastRewardedCycle = PlayerPrefs.GetInt(KeyPrefs.RewardLastCycle, -1);
            if (currentCycle <= lastRewardedCycle) return;

            UIEvent.OnSetWinButtonsInteractable?.Invoke(false);
            GiveSpearReward(currentLevel);

            PlayerPrefs.SetInt(KeyPrefs.RewardLastCycle, currentCycle);
            PlayerPrefs.Save();

            DOVirtual.DelayedCall(resetDelay, ResetBar, ignoreTimeScale: true);
        }

        private void ResetBar()
        {
            fillBar.DOKill();
            fillBar.DOFillAmount(0, fillDuration * 0.5f).SetEase(Ease.InCubic).SetUpdate(true);
            for (int i = 0; i < rewardSlotUIs.Count; i++)
                if (rewardSlotUIs[i] != null) rewardSlotUIs[i].SetupSlotState(false);
            UIEvent.OnSetWinButtonsInteractable?.Invoke(true);
        }

        private void GiveSpearReward(int currentLevel)
        {
            if (allSpearSkinDataSO == null || allSpearSkinDataSO.AllSpearSkinDataList == null)
            {
                GamePrefs.Coin += fallbackCoin;
                return;
            }

            // Ưu tiên 1: spear có unlockLevel == currentLevel, chưa sở hữu
            foreach (var spear in allSpearSkinDataSO.AllSpearSkinDataList)
            {
                if (spear.unlockLevel == currentLevel && !GamePrefs.IsSpearSkinOwned(spear.skinNameID))
                {
                    GamePrefs.SetSpearSkinOwned(spear.skinNameID);
                    UIEvent.OnShowRewardPopup?.Invoke(spear.skinNameID);
                    return;
                }
            }

            // Ưu tiên 2: spear unlockLevel == 0, chưa sở hữu
            foreach (var spear in allSpearSkinDataSO.AllSpearSkinDataList)
            {
                if (spear.unlockLevel == 0 && !GamePrefs.IsSpearSkinOwned(spear.skinNameID))
                {
                    GamePrefs.SetSpearSkinOwned(spear.skinNameID);
                    UIEvent.OnShowRewardPopup?.Invoke(spear.skinNameID);
                    return;
                }
            }

            // Fallback: mở popup reward coin
            UIEvent.OnShowCoinRewardPopup?.Invoke(fallbackCoin);
        }
    }
}
