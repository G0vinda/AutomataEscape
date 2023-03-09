using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TransitionPlaceElement : MonoBehaviour
    {
        private Image _image; 
        private UIManager _uiManager;
        private TransitionUIData _data;
        private Vector3 _dragZOffset = new (0f, 0f, 2f);
        public void Initialize(UIManager uiManager, TransitionUIData transitionUIData)
        {
            _image = GetComponent<Image>();
            _uiManager = uiManager;
            _data = transitionUIData;
            _image.color = transitionUIData.color;
        }

        private void Update()
        {
            transform.position = Input.mousePosition + _dragZOffset;
            if (Input.GetMouseButtonUp(0))
            {
                _uiManager.HandleTransitionPlaceElementReleased(_data);
                Destroy(gameObject);
            }
        }
    }
}
