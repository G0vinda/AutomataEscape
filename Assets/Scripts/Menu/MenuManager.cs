using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Menu
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject newGamePrompt;
        [SerializeField] private Button continueButton;

        private const int LevelSelectionSceneIndex = 1;
        private const int MainSceneIndex = 2;
        private bool _playerHasReachedLevel;

        private void Start()
        {
            _playerHasReachedLevel = PlayerPrefs.GetInt("ReachedLevelId", -1) >= 0;
            continueButton.interactable = _playerHasReachedLevel;
        }

        public void ContinueGame()
        {
            SceneManager.LoadScene(LevelSelectionSceneIndex);
        }

        public void RequestStartNewGame()
        {
            if (_playerHasReachedLevel)
            {
                newGamePrompt.SetActive(true);
            }
            else
            {
                StartNewGame();
            }
        }

        public void StartNewGame()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetInt("ReachedLevelId", 0);
            SceneManager.LoadScene(MainSceneIndex);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
