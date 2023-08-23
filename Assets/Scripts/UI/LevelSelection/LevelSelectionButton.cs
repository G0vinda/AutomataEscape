using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.LevelSelection
{
    public class LevelSelectionButton : MonoBehaviour
    {
        [SerializeField] private Color unlockedColor;
        [SerializeField] private Color lockedColor;
        [SerializeField] private SpriteRenderer buttonSpriteRenderer;
        [SerializeField] private GameObject marking;

        private LevelSelectionManager _levelSelectionManager;
        private bool _locked;

        private void Awake()
        {
            _levelSelectionManager = GetComponentInParent<LevelSelectionManager>();
        }

        public void Lock()
        {
            _locked = true;
            buttonSpriteRenderer.color = lockedColor;
        }

        public void Unlock()
        {
            _locked = false;
            buttonSpriteRenderer.color = unlockedColor;
        }

        public void SetMarking(bool value)
        {
            marking.SetActive(value);   
        }

        private void OnMouseDown()
        {
            if (_locked)
                return;

            _levelSelectionManager.LoadLevel(this);
        }
    }
}
