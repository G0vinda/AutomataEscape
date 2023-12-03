using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.LevelSelection
{
    public class LevelSelectionButton : MonoBehaviour
    {
        [SerializeField] private Animator buttonAnimator;
        [SerializeField] private Animator gearAnimator;
        [SerializeField] private GameObject marking;
        
        private LevelSelectionManager _levelSelectionManager;
        private Image _gearSpriteRenderer;
        private bool _locked;
        private int _levelSelectIsOpenHash;
        private int _levelSelectIsClosedHash;
        private int _openLevelSelectHash;
        private int _openGearHash;

        private void Awake()
        {
            _levelSelectionManager = GetComponentInParent<LevelSelectionManager>();
            _gearSpriteRenderer = gearAnimator.GetComponent<Image>();
            
            //Button
            _levelSelectIsOpenHash = Animator.StringToHash("LevelSelectIsOpen");
            _levelSelectIsClosedHash = Animator.StringToHash("LevelSelectIsClosed");
            _openLevelSelectHash = Animator.StringToHash("LevelSelectOpen");
            
            //Gear
            _openGearHash = Animator.StringToHash("openGear");
        }

        public void SetLockState(bool lockState)
        {
            _locked = lockState;
            _gearSpriteRenderer.enabled = _locked;
            var animationState = _locked ? _levelSelectIsClosedHash : _levelSelectIsOpenHash;
            buttonAnimator.CrossFade(animationState, 0, 0);
        }

        public void Unlock()
        {
            SoundPlayer.Instance.PlayLevelUnlock();
            buttonAnimator.CrossFade(_openLevelSelectHash, 0, 0);
            gearAnimator.CrossFade(_openGearHash, 0, 0);
            _locked = false;
        }

        public void SetMarking(bool value)
        {
            marking.SetActive(value);   
        }

        public void OnClick()
        {
            if(_locked)
                return;
            
            SoundPlayer.Instance.PlayButtonClickMenu();
            _levelSelectionManager.LoadLevel(this);
        }
    }
}
