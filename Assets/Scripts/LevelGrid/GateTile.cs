using System;
using DG.Tweening;
using UI.Transition;
using UnityEngine;

namespace LevelGrid
{
    public class GateTile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer tileRenderer;
        [SerializeField] private GameObject gateWall;
        [SerializeField] private GameObject placedKey;
        [SerializeField] private LevelGridManager.KeyType neededKeyType;

        private Direction _gateDirection;
        private bool _locked = true;
        private float _unlockAnimationTime = 1f;
        
        public void Initialize(Direction newDirection, int wallSpriteSortingOrder)
        {
            gateWall.GetComponent<SpriteRenderer>().sortingOrder = wallSpriteSortingOrder;
            switch (newDirection)
            {
                case Direction.Right:
                    var wallPosition = gateWall.transform.localPosition;
                    wallPosition.x = -wallPosition.x;
                    gateWall.transform.localPosition = wallPosition;
                    
                    var placedKeyPosition = placedKey.transform.localPosition;
                    placedKeyPosition.x = -placedKeyPosition.x;
                    placedKey.transform.localPosition = placedKeyPosition;
                    
                    tileRenderer.flipX = true;
                    break;
                case Direction.Left:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _gateDirection = newDirection;
        }
        
        public bool Unlock(LevelGridManager.KeyType keyType)
        {
            if (keyType != neededKeyType)
                return false;
            
            _locked = false;
            placedKey.SetActive(true);
            var gateWallSpriteRenderer = gateWall.GetComponent<SpriteRenderer>();
            var gateWallColor = gateWallSpriteRenderer.color;
            
            var unlockSequence = DOTween.Sequence();
            unlockSequence.Append(placedKey.transform.DORotate(new Vector3(0, 0, 100f), _unlockAnimationTime).From());
            unlockSequence.Join(DOVirtual.Float(1, 0, _unlockAnimationTime, value =>
            {
                gateWallColor.a = value;
                gateWallSpriteRenderer.color = gateWallColor;
            }));
            unlockSequence.SetEase(Ease.OutSine).OnComplete(() => { gateWall.SetActive(false); });
            unlockSequence.Play();
            SoundPlayer.Instance.PlayOpenGate();

            return true;
        }

        public bool IsBlockingWay(Direction direction)
        {
            return direction == _gateDirection && _locked;
        }
    }
}
