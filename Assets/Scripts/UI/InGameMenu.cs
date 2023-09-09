using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InGameMenu : MonoBehaviour
    {
        [SerializeField] private GameObject runButton;
        [SerializeField] private GameObject viewButton;
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
            viewButton.SetActive(!value);
            inputManager.enabled = !value;

            if (value)
            {
                sfxSlider.value = SoundPlayer.Instance.GetSfxVolume();
                musicSlider.value = SoundPlayer.Instance.GetMusicVolume();
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
            SoundPlayer.Instance.UpdateSfxVolume(newVolume);
        }

        public void MusicSliderChanged(float newVolume)
        {
            PlayerPrefs.SetFloat("MusicVolume", newVolume);
            SoundPlayer.Instance.UpdateMusicVolume(newVolume);
        }
    }
}
