using Helper;
using UI;
using UnityEngine;

namespace Tiles
{
    public class GateTile : Tile
    {
        [SerializeField] private GameObject gate;

        private Direction _gateDirection;
        private bool _locked = true;

        public void SetDirection(Direction newDirection)
        {
            _gateDirection = newDirection;
            transform.rotation = newDirection.ToZRotation();
        }
        
        public void Unlock()
        {
            _locked = false;
            gate.SetActive(false);
        }

        public bool IsBlockingWay(Vector2 direction)
        {
            return true; // Todo: this will need to be handled: direction == _gateDirection && _locked;
        }
    }
}
