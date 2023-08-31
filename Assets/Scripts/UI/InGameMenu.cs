using UnityEngine;

namespace UI
{
    public class InGameMenu : MonoBehaviour
    {
        [SerializeField] private GameObject runButton;
        [SerializeField] private GameObject viewButton;
        [SerializeField] private InputManager inputManager;
        public void ToggleMenu()
        {
            SetMenuActive(!gameObject.activeSelf);
        }

        private void SetMenuActive(bool value)
        {
            gameObject.SetActive(value);
            runButton.SetActive(!value);
            viewButton.SetActive(!value);
            inputManager.enabled = !value;
        }

        public void CloseMenu()
        {
            SetMenuActive(false);
        }

        public void PlayButtonClick()
        {
            SoundPlayer.Instance.PlayButtonClickMenu();
        }

        public void SfxSliderChanged(float newVolume)
        {
            SoundPlayer.Instance.UpdateSfxVolume(newVolume);
        }

        public void MusicSliderChanged(float newVolume)
        {
            SoundPlayer.Instance.UpdateMusicVolume(newVolume);
        }
    }
}
