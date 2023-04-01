using System;
using UI;
using UnityEngine;

namespace Tiles
{
    public class GateTile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer tileRenderer;
        [SerializeField] private GameObject gateWall;
        [SerializeField] private GameObject placedKeyIndicator;

        public GridManager.KeyType neededKeyType;

        private Direction _gateDirection;
        private bool _locked = true;
        
        public void SetDirection(Direction newDirection)
        {
            switch (newDirection)
            {
                case Direction.Right:
                    var wallPosition = gateWall.transform.localPosition;
                    wallPosition.x = -wallPosition.x;
                    gateWall.transform.localPosition = wallPosition;
                    var placedKeyPosition = placedKeyIndicator.transform.localPosition;
                    placedKeyPosition.x = -placedKeyPosition.x;
                    placedKeyIndicator.transform.localPosition = placedKeyPosition;
                    tileRenderer.flipX = true;
                    break;
                case Direction.Left:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _gateDirection = newDirection;
        }
        
        public void Unlock()
        {
            _locked = false;
            gateWall.SetActive(false);
            placedKeyIndicator.SetActive(true);
        }

        public bool IsBlockingWay(Direction direction)
        {
            return direction == _gateDirection && _locked;
        }
    }
}
