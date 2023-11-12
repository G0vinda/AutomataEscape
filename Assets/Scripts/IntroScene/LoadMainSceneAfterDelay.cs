using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace IntroScene
{
    public class LoadMainSceneAfterDelay : MonoBehaviour
    {
        [SerializeField] private float delayInMilliseconds;

        private const int MainSceneIndex = 3;

        private void Start()
        {
            var delay = delayInMilliseconds / 60;
            StartCoroutine(LoadMainSceneAfterDelayInSeconds(delay));
        }

        private IEnumerator LoadMainSceneAfterDelayInSeconds(float delay)
        {
            yield return new WaitForSeconds(delay); // time of the cutscene

            var scene = SceneManager.LoadSceneAsync(MainSceneIndex);
            scene.allowSceneActivation = false;

            while (scene.progress < 0.9f)
            {
                yield return null;
            }

            scene.allowSceneActivation = true;
        }
    }
}
