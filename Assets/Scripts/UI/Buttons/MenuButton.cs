using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Buttons
{
    public class MenuButton : MonoBehaviour
    {
        [SerializeField] private int MenuSceneIndex;
    
        public void ReturnToMenu()
        {
            SceneManager.LoadScene(MenuSceneIndex);
        }
    }
}
