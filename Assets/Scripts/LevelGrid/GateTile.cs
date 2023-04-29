using System;
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
            gateWall.SetActive(false);
            placedKey.SetActive(true);
            return true;
        }

        public bool IsBlockingWay(Direction direction)
        {
            return direction == _gateDirection && _locked;
        }
    }
}