using UnityEngine;

namespace UI
{
    public class UIMenuButton : MonoBehaviour
    {
        public void PlayClickSound()
        {
            SoundPlayer.Instance.PlayButtonClickMenu();
        }
    }
}
