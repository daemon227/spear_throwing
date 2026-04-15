
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DongA.UI
{
    public class FirstLoadUI : MonoBehaviour
    {
        [Header("Start Loading Elements")]
        public CanvasGroup loadingPanel;
        public Image loadingBar;
        
        [Header("Settings")]
        public float fillDuration = 0.75f;
        public float fadeDuration = 0.75f;      

        void Start()
        {
            StartLoading();
        }

        private void StartLoading()
        {
            
            loadingBar.fillAmount = 0;
            loadingPanel.gameObject.SetActive(true);

            Sequence sequence = DOTween.Sequence();

            sequence.Append(loadingPanel.DOFade(1, 0f));
            
            sequence.Append(loadingBar.DOFillAmount(0.7f, fillDuration * 0.5f)
                .SetEase(Ease.OutQuad));
            
            sequence.Append(loadingBar.DOFillAmount(0.8f, fillDuration * 1.5f)
                .SetEase(Ease.Linear));
            
            sequence.Append(loadingBar.DOFillAmount(1f, fillDuration * 0.25f)
                .SetEase(Ease.OutQuad));

            sequence.OnComplete(() =>
            {
                //DOTween.KillAll();
                // SceneManager.LoadScene("Game");
            });

        }
    }
}
