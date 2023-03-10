using TMPro;
using UnityEngine;

namespace UI
{
    public class RunButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textObject;

        private void OnEnable()
        {
            ListenToGameManager();
        }

        private void ListenToGameManager()
        {
            GameManager.Instance.StateChartRunnerStateChanged += ToggleText;   
        }
        
        private void OnDisable()
        {
            GameManager.Instance.StateChartRunnerStateChanged -= ToggleText;
        }

        private void ToggleText(bool isRunning)
        {
            textObject.text = isRunning ? "Stop" : "Run";
        }
    }
}
