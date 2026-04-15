using System.Collections;
using System.Collections.Generic;
using DongA.GameDatas;
using Unity.VisualScripting;
using UnityEngine;

namespace GameBase
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;
        public SoundSO soundSO;
        public bool IsSoundOn = true;

        private AudioSource _enemyScaredAudioSource;
        private AudioSource _enemyDeadAudioSource;
        private AudioSource _reflectAudioSource;
        private AudioSource _uiAudioSource;
        private AudioSource _bgmAudioSource;


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            _enemyScaredAudioSource = gameObject.AddComponent<AudioSource>();
            _enemyScaredAudioSource.volume = 0.8f;
            _enemyDeadAudioSource = gameObject.AddComponent<AudioSource>();
            _enemyDeadAudioSource.volume = 0.8f;
            _reflectAudioSource = gameObject.AddComponent<AudioSource>();
            _reflectAudioSource.volume = 0.6f;
            _uiAudioSource = gameObject.AddComponent<AudioSource>();
            _uiAudioSource.volume = 0.8f;
            _bgmAudioSource = gameObject.AddComponent<AudioSource>();
            _bgmAudioSource.volume = 0.35f;
        }

        void Start()
        {
            GameEvents.OnMusicStateChanged += PlayBGMSound;
            PlayBGMSound(GlobalValues.MusicEnable);
        }

        private void OnDestroy()
        {
            GameEvents.OnMusicStateChanged -= PlayBGMSound;
        }

        public void PlayBGMSound(bool isPlay)
        {
            if (isPlay)
            {
                if (_bgmAudioSource.isPlaying) return;
                var clip = soundSO.bgm;
                if (clip == null) return;
                _bgmAudioSource.clip = clip;
                _bgmAudioSource.Play();
                _bgmAudioSource.loop = true;
            }
            else
            {
                _bgmAudioSource.Stop();
            }
        }

        public bool PlayEnemyScaredSound()
        {
            if (!GlobalValues.SoundEnable || !GlobalValues.ShowEnemySound) return false;
            if (_enemyScaredAudioSource.isPlaying) return false;

            var clip = soundSO.characterScared;
            if(Random.value < 0.5f)
            {
                clip = soundSO.characterScared2;
            }
           
            if (clip == null) return false;

            _enemyScaredAudioSource.clip = clip;
            _enemyScaredAudioSource.Play();
            return true;
        }

        public bool PlayEnemyDeadSound(AudioClip clip)
        {
            if (!GlobalValues.SoundEnable || clip == null) return false;
            if (_enemyDeadAudioSource.isPlaying) return false;

            _enemyDeadAudioSource.clip = clip;
            _enemyDeadAudioSource.Play();
            return true;
        }

        public void PlayReflectSound()
        {
            if (!GlobalValues.SoundEnable || soundSO.reflect == null) return;
            _reflectAudioSource.pitch = Random.Range(0.85f, 1.2f);
            _reflectAudioSource.clip = soundSO.reflect;
            _reflectAudioSource.Play();
        }

        public void PlayUISound()
        {
            if (!GlobalValues.SoundEnable || soundSO.buttonClick == null) return;
            _uiAudioSource.PlayOneShot(soundSO.buttonClick);
        }

        public void PlaySoundOnShot(AudioSource source, AudioClip clip)
        {
            if (GlobalValues.SoundEnable && clip != null)
            {
                source.PlayOneShot(clip);
            }
        }
        public void PlaySoundAtPoint(AudioClip clip, Vector3 worldPosition, float volume = 1f)
        {
            if (GlobalValues.SoundEnable && clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, worldPosition, volume);
            }
        }

        public void PlaySoundLoop(AudioSource source, AudioClip clip)
        {
            if (!GlobalValues.SoundEnable || clip == null) return;
            if (source.clip != clip)
            {
                source.clip = clip;
                source.loop = true;
                source.Play();
            }
            
        }
    }
}


