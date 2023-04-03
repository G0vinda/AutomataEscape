using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Menu
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private Button continueButton;

        private const int MainSceneIndex = 1;

        private void Start()
        {
            continueButton.interactable = PlayerPrefs.GetInt("CurrentLevelId", 0) > 0;
        }

        public void ContinueGame()
        {
            SceneManager.LoadScene(MainSceneIndex);
        }

        public void StartNewGame()
        {
            PlayerPrefs.SetInt("CurrentLevelId", 0);
            SceneManager.LoadScene(MainSceneIndex);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
