using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DongA.GameDatas
{
    [CreateAssetMenu(fileName = "SoundSO", menuName = "GameDatas/SoundSO")]
    public class SoundSO : ScriptableObject
    {
        [Header("BGM")]
        public AudioClip bgm;

        [Header("Spear Sounds")]
        public AudioClip spearActive;
        public AudioClip spearLaunch;
        public AudioClip spearStuck;
        public AudioClip fireSpear;

        [Header("Character Sounds")]
        public AudioClip characterScared;
        public AudioClip characterScared2;


        [Header("Dead Sounds")]
        public AudioClip deadByBurn;
        public AudioClip deadByElectric;
        public AudioClip deadBySlash;

        [Header("Coin Sounds")]
        public AudioClip coinCollect;
        public AudioClip coinBreak;

        [Header("Obstacle Sounds")]
        public AudioClip boom;
        public AudioClip iceBreak;
        public AudioClip ironBreak;
        public AudioClip balloonBreak;
        public AudioClip reflect;

        [Header("Powerup Sounds")]
        public AudioClip powerupCollect;


        [Header("UI Sounds")]
        public AudioClip buttonClick;
        public AudioClip WinLevel;
        public AudioClip LoseLevel;

    }
}
