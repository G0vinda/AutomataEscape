using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UI
{
    public class LevelInfoText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textObject;

        [Header("Text effect values")] 
        [SerializeField] private float timePerLetter;
        [SerializeField] private int iterationsPerLetter;

        private WaitForSeconds _iterationWait;
        
        private void Awake()
        {
            _iterationWait = new WaitForSeconds(timePerLetter);
        }

        public void ShowLevelInfo(int levelId)
        {
            gameObject.SetActive(true);
            StartCoroutine(PlayTextEffect($"Level {levelId + 1}"));
        }

        private IEnumerator PlayTextEffect(string text)
        {
            var currentTextArray = new char[text.Length];
            for (var i = 0; i < text.Length; i++)
            {
                for (var j = 0; j < iterationsPerLetter; j++)
                {
                    var randomChar = (char)Random.Range(33, 126);
                    currentTextArray[i] = randomChar;

                    textObject.text = new string(currentTextArray);
                    yield return _iterationWait;
                }

                currentTextArray[i] = text.ElementAt(i);
            }
            textObject.text = new string(currentTextArray);
        }
    }
}
