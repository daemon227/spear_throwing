using UnityEngine;
using UnityEngine.SceneManagement;

namespace DongA.UI
{
    public class LevelPackItemUI : MonoBehaviour
    {
        [SerializeField] private int levelThemeID;
        void Awake()
        {
            GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnThisOptionSelected);
        }

        private void OnThisOptionSelected()
        {
            GamePrefs.ThemeID = levelThemeID;
            SceneManager.LoadScene("Game");
        }
    }
}
