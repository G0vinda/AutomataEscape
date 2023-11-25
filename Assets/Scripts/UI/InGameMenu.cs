using UI.Buttons;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InGameMenu : MonoBehaviour
    {
        [SerializeField] private GameObject runButton;
        [SerializeField] private ViewButton viewButton;
        [SerializeField] private GameObject userManualButton;
        [SerializeField] private GameObject userManual;
        [SerializeField] private InputManager inputManager;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider musicSlider;

        public void ToggleMenu()
        {
            SetMenuActive(!gameObject.activeSelf);
        }

        private void SetMenuActive(bool value)
        {
            gameObject.SetActive(value);
            runButton.SetActive(!value);
            viewButton.gameObject.SetActive(!value);
            userManualButton.SetActive(!value);
            inputManager.enabled = !value;

            if (value)
            {
                sfxSlider.value = SoundPlayer.Instance.GetSfxVolume();
                musicSlider.value = SoundPlayer.Instance.GetMusicVolume();
                userManual.SetActive(false);
            }
            else
            {
                viewButton.ListenToRobotGotClickedEvent(true);
            }
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
            PlayerPrefs.SetFloat("SfxVolume", newVolume);
            SoundPlayer.Instance.UpdateSfxVolume();
        }

        public void MusicSliderChanged(float newVolume)
        {
            PlayerPrefs.SetFloat("MusicVolume", newVolume);
            SoundPlayer.Instance.UpdateMusicVolume();
        }
    }
}
