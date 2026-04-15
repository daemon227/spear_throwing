using DG.Tweening;
using UnityEngine;

namespace DongA.UI
{
    public class PopupBase : MonoBehaviour
    {
        [SerializeField] protected Canvas canvas;
        [SerializeField] protected Canvas backgroundCanvas;
        [SerializeField] protected GameObject backgroundPanel;
        [SerializeField] protected GameObject popup;

        // void Awake()
        // {
        //     InitializePopup();
        // }

        public virtual void InitializePopup()
        {
            // canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("PopupBase: No Canvas component found!");
                return;
            }
            if(backgroundCanvas != null) backgroundCanvas.worldCamera = Camera.main;
            canvas.worldCamera = Camera.main;
            //backgroundCanvas.worldCamera = Camera.main;
            canvas.sortingLayerID = SortingLayer.NameToID(GameTags.SLUIPopup);
        }
        public virtual void ShowPopup()
        {
            InitializePopup();
            if (backgroundPanel != null) backgroundPanel.SetActive(true);
            if (popup != null)
            {
                popup.SetActive(true);
                popup.transform.DOScale(1, 0.45f).From(0).SetEase(Ease.OutBack).SetUpdate(true);
            }
            GamePrefs.IsGamePaused = true;
            Time.timeScale = 0f;
        }
        public void HidePopup(bool resumeGame = true)
        {
            if (backgroundPanel != null) backgroundPanel.SetActive(false);
            if (popup != null) popup.SetActive(false);
            if (resumeGame)
            {
                GamePrefs.IsGamePaused = false;
                Time.timeScale = 1f;
                if (!GamePrefs.IsIceFrozen)
                    DOTween.PlayAll();
            }
        }
    }
    public enum PopupType
    {
        Win,
        Lose,
        Pause,
        Shop,
        NoAds,
        Reward,
        NoInternet,
        
    }
}

