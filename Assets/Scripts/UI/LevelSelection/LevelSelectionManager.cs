using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.LevelSelection
{
    public class LevelSelectionManager : MonoBehaviour
    {
        [SerializeField] private LevelSelectionButton[] selectionButtons;
        [SerializeField] private ScrollRect scrollRect;

        private const int MainSceneIndex = 2;

        private void Start()
        {
            var reachedLevel = PlayerPrefs.GetInt("ReachedLevelId", 0);
            var markedButtonPosition = selectionButtons[reachedLevel].transform.position;
            AlignScrollContentToPosition(markedButtonPosition.x);

            for (var i = 0; i < selectionButtons.Length; i++)
            {
                if (i <= reachedLevel)
                {
                    selectionButtons[i].Unlock();
                }
                else
                {
                    selectionButtons[i].Lock();
                }
                
                selectionButtons[i].SetMarking(i == reachedLevel);
            }
        }

        private void AlignScrollContentToPosition(float xValue)
        {
            var contentWidth = GetComponent<RectTransform>().sizeDelta.x;
            var halfContentWidth = contentWidth * 0.5f;
            scrollRect.horizontalNormalizedPosition = xValue / contentWidth;
        }

        public void LoadLevel(LevelSelectionButton selectionButton)
        {
            var levelIndex = Array.IndexOf(selectionButtons, selectionButton);
            
            PlayerPrefs.SetInt("CurrentLevelId", levelIndex);
            SceneManager.LoadScene(MainSceneIndex);
        }
    }
}
