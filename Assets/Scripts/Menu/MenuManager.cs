using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Menu
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private Button continueButton;

        private const int LevelSelectionSceneIndex = 1;
        private const int MainSceneIndex = 2;

        private void Start()
        {
            continueButton.interactable = PlayerPrefs.GetInt("ReachedLevelId", 0) > 0;
        }

        public void ContinueGame()
        {
            SceneManager.LoadScene(LevelSelectionSceneIndex);
        }

        public void StartNewGame()
        {
            PlayerPrefs.SetInt("ReachedLevelId", 0);
            PlayerPrefs.SetInt("CurrentLevelId", 0);
            SceneManager.LoadScene(MainSceneIndex);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
