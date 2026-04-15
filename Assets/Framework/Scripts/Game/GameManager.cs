using System.Collections;
using System.Collections.Generic;
using DongA.Events;
using UnityEngine;
using DongA.UI;

namespace GameBase
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public GameState CurrentGameState = GameState.Playing;
        private int numberEnemiesInLevel = 0;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

        }

        private void Start()
        {
            AdsUtils.ShowBanner();
        }
        void OnEnable()
        {
            UIEvent.OnNextLevelButtonClicked += OnNextLevel;
            UIEvent.OnReplayLevelButtonClicked += OnReplayLevel;
        }
        void OnDisable()
        {
            UIEvent.OnNextLevelButtonClicked -= OnNextLevel;
            UIEvent.OnReplayLevelButtonClicked -= OnReplayLevel;
        }

        public void IncreaseEnemyCount()
        {
            numberEnemiesInLevel++;
        }
        [Header("Last Kill Slow Motion")]
        [SerializeField] private float slowMotionTimeScale = 0.2f;
        [SerializeField] private float slowMotionDuration = 1.0f;

        private Vector3 _lastKillPosition;

        public void DecreaseEnemyCount(Vector3 killPosition = default)
        {
            _lastKillPosition = killPosition;
            numberEnemiesInLevel--;
            if (numberEnemiesInLevel <= 0 && CurrentGameState == GameState.Playing)
            {
                // All enemies defeated, trigger win condition
                CurrentGameState = GameState.Win;
                GamePrefs.IsGamePaused = true;
                StartCoroutine(SlowMotionThenShowWinPopup());
            }
        }

        private IEnumerator SlowMotionThenShowWinPopup()
        {
            GameEvents.OnKillSlowMoStart?.Invoke(_lastKillPosition);
            Time.timeScale = slowMotionTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            yield return new WaitForSecondsRealtime(slowMotionDuration);

            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            GameEvents.OnKillSlowMoEnd?.Invoke();

            yield return new WaitForSeconds(1.5f);
            UIEvent.OnShowPopup?.Invoke(PopupType.Win);

            GlobalValues.FinishPlay(true);
            
            if(SoundManager.Instance != null)
            {
                var audioSource = GetComponent<AudioSource>();
                if(audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
                SoundManager.Instance.PlaySoundOnShot(audioSource, SoundManager.Instance.soundSO.WinLevel);
            }
        }

        private IEnumerator DelayBeforeShowPopup(float delay, PopupType popupType)
        {
            yield return new WaitForSeconds(delay);
            UIEvent.OnShowPopup?.Invoke(popupType);
        }
        private void ResetGameState()
        {
            CurrentGameState = GameState.Playing;
            GamePrefs.IsGamePaused = false;
            numberEnemiesInLevel = 0;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
        public void OnNextLevel()
        {
            GamePrefs.Level++;
            ResetGameState();
            GameEvents.OnLevelStartLoading?.Invoke();
        }
        public void TriggerLose()
        {
            if (CurrentGameState != GameState.Playing) return;
            CurrentGameState = GameState.Lose;
            GamePrefs.IsGamePaused = true;

            if(SoundManager.Instance != null)
            {
                var audioSource = GetComponent<AudioSource>();
                if(audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
                SoundManager.Instance.PlaySoundOnShot(audioSource, SoundManager.Instance.soundSO.LoseLevel);
            }

            StartCoroutine(DelayBeforeShowPopup(1f, PopupType.Lose));
            GlobalValues.FinishPlay(false);
        }

        public void OnReplayLevel()
        {
            ResetGameState();
            GameEvents.OnLevelStartLoading?.Invoke();
        }
    }
    public enum GameState
    {
        Playing,
        Paused,
        Win,
        Lose
    }
}
