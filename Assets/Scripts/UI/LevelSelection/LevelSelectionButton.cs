using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.LevelSelection
{
    public class LevelSelectionButton : MonoBehaviour
    {
        [SerializeField] private Button button;
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
            button.interactable = false;
        }

        public void Unlock()
        {
            _locked = false;
            button.interactable = true;
        }

        public void SetMarking(bool value)
        {
            marking.SetActive(value);   
        }

        public void OnClick()
        {
            _levelSelectionManager.LoadLevel(this);
        }
    }
}
