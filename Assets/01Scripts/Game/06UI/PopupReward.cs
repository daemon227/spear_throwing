using System.Collections;
using System.Runtime.InteropServices;
using DG.Tweening;
using DongA.Events;
using DongA.GameDatas;
using Spine.Unity;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace DongA.UI
{
    public class PopupReward : PopupBase
    {
        [SerializeField] private SkeletonGraphic _chestUI;
        [SerializeField] private GameObject _uiPopup;
        [SerializeField] private Image _rewardImage;
        [SerializeField] private TextMeshProUGUI _spearName;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button claimButton;

        [SerializeField] private AllSpearSkinDataSO allSpearSkinDataSO;
        [SerializeField] private float _uiPopupDelay = 1f;
        [SerializeField] private float _flyDistance = 200f;
        [SerializeField] private float _flyDuration = 0.5f;

        private string _pendingSpearNameID;
        private int _pendingCoinReward;
        private bool _isCoinReward;

        private void Awake()
        {
            _uiPopup.SetActive(false);
            claimButton.gameObject.SetActive(false);
            closeButton.onClick.AddListener(OnCloseClicked);
            claimButton.onClick.AddListener(OnCloseClicked);
        }
        public override void InitializePopup()
        {
            base.InitializePopup();
        }

        private void OnEnable()
        {
            _chestUI.AnimationState.SetAnimation(0, "Final/final", false);
            StartCoroutine(ShowUIPopupAfterDelay());
        }

        private IEnumerator ShowUIPopupAfterDelay()
        {
            yield return new WaitForSecondsRealtime(_uiPopupDelay);
            _chestUI.AnimationState.SetAnimation(0, "Final/idle", true);

            RectTransform popupRect = _uiPopup.GetComponent<RectTransform>();
            RectTransform parentRect = popupRect.parent as RectTransform;

            Vector2 startPos = parentRect != null
                ? (Vector2)parentRect.InverseTransformPoint(_chestUI.transform.position)
                : (Vector2)_chestUI.transform.position;
            Vector2 endPos = startPos + new Vector2(0f, _flyDistance);

            _uiPopup.SetActive(true);
            _uiPopup.transform.localScale = Vector3.zero;
            popupRect.anchoredPosition = startPos;

            var sequence = DOTween.Sequence();

            sequence.Join(popupRect.DOAnchorPos(endPos, _flyDuration).SetEase(Ease.OutCubic))
            .Join(_uiPopup.transform.DOScale(1f, _flyDuration).SetEase(Ease.OutBack))
            .SetUpdate(true);
            sequence.OnComplete(() =>
            {
                claimButton.gameObject.SetActive(true);
            });
        }

        public void Setup(string spearNameID)
        {
            _isCoinReward = false;
            _pendingSpearNameID = spearNameID;

            var spearData = allSpearSkinDataSO.AllSpearSkinDataList.Find(s => s.skinNameID == spearNameID);
            if (spearData == null) return;

            if (_rewardImage != null && spearData.skinInRewardUI != null)
                _rewardImage.sprite = spearData.skinInRewardUI;

            if (_spearName != null)
                _spearName.text = spearData.skinName;

            // Equip spear ngay khi nhận reward
            GamePrefs.CurrentSpearSkinID = spearNameID;
            GamePrefs.CurrentSpearSkinName = spearData.SkinName;
            GamePrefs.CurrentSpearType = (int)spearData.SpearType;
            GameEvents.OnSpearSkinChosen?.Invoke();
        }

        public void SetupCoinReward(int coinAmount)
        {
            _isCoinReward = true;
            _pendingCoinReward = coinAmount;

            if (_spearName != null)
                _spearName.text = coinAmount + " coins";
        }

        private void OnCloseClicked()
        {
            if (_isCoinReward)
            {
                UIEvent.OnCoinRewardClaimed?.Invoke(_pendingCoinReward);
            }
            

            UIEvent.OnClosePopupButtonClicked?.Invoke();
        }
    }
}

