using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DongA.Game
{
    public class ThemeManager : MonoBehaviour
    {
        public static ThemeManager Instance;
        public GameObject currentTheme;
        public GameObject mktTheme;
        public List<Canvas> canvasList;
        public List<Image> images;

        void Awake()
        {
            Instance = this;
        }
        void UpdateCanvas()
        {
            if (canvasList == null || canvasList.Count==0) return;
            foreach (var canvas in canvasList)
            {
                canvas.worldCamera = Camera.main;
            }
        }

        public void UpdateBackgroundColor(Color color)
        {
            UpdateCanvas();
            if(images == null || images.Count == 0) return;
            foreach (var image in images)
            {
                image.color = color;
            }
            mktTheme.SetActive(true);
            currentTheme.SetActive(false);
        }

        public void DisableMKTTheme()
        {
            if (mktTheme != null) mktTheme.SetActive(false);
            if (currentTheme != null) currentTheme.SetActive(true);
        }

        public bool IsMKTThemeActive => mktTheme != null && mktTheme.activeSelf;
    }
}

