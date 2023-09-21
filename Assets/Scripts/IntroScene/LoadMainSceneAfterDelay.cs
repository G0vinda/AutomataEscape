using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IntroScene
{
    public class LoadMainSceneAfterDelay : MonoBehaviour
    {
        [SerializeField] private float delayInMiliseconds;

        private const int MainSceneIndex = 3;

        private void Start()
        {
            var delay = delayInMiliseconds / 60;
            Debug.Log($"Delay is {delay}");
            Invoke(nameof(LoadMainScene), delay);
        }

        private void LoadMainScene()
        {
            SceneManager.LoadScene(MainSceneIndex);
        }
    }
}
