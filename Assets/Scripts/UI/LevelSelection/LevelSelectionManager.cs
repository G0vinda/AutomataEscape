using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.LevelSelection
{
    public class LevelSelectionManager : MonoBehaviour
    {
        [SerializeField] private LevelSelectionButton[] selectionButtons;
        [SerializeField] private LineRenderer line;
        [SerializeField] private int testReachedLevel;

        private const int MainSceneIndex = 2;

        private void Start()
        {
            //PlayerPrefs.SetInt("ReachedLevelId", testReachedLevel);
            var reachedLevel = PlayerPrefs.GetInt("ReachedLevelId", 0);
            
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

            var selectionButtonPositions = selectionButtons.Select(button => button.transform.position).ToArray();
            line.positionCount = selectionButtonPositions.Length;
            line.SetPositions(selectionButtonPositions);
        }

        public void LoadLevel(LevelSelectionButton selectionButton)
        {
            var levelIndex = Array.IndexOf(selectionButtons, selectionButton);
            
            PlayerPrefs.SetInt("CurrentLevelId", levelIndex);
            SceneManager.LoadScene(MainSceneIndex);
        }
    }
}
