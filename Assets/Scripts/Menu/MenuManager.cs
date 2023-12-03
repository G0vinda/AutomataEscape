using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Menu
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject newGamePrompt;
        [SerializeField] private Button continueButton;
        [SerializeField] private GameObject loadingScreen;

        private const int LevelSelectionSceneIndex = 1;
        private const int IntroSceneIndex = 2;
        private bool _playerHasReachedLevel;

        private void Start()
        {
            _playerHasReachedLevel = PlayerPrefs.GetInt("ReachedLevelId", -1) >= 0;
            continueButton.interactable = _playerHasReachedLevel;
        }

        public void ContinueGame()
        {
            SoundPlayer.Instance.PlayAtmoLevel();
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

            StartCoroutine(LoadIntroScene());
        }

        private IEnumerator LoadIntroScene()
        {
            var scene = SceneManager.LoadSceneAsync(IntroSceneIndex);
            scene.allowSceneActivation = false;
            loadingScreen.SetActive(true);

            while (scene.progress < 0.9f)
            {
                yield return null;
            }

            scene.allowSceneActivation = true;
            SoundPlayer.Instance.StopMusic();
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
