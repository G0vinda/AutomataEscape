using Helper;
using UnityEngine;

namespace Tiles
{
    public class GateTile : Tile
    {
        [SerializeField] private GameObject gate;

        private Vector2 _gateDirection;
        private bool _locked = true;

        public void SetDirection(Vector2 newDirection)
        {
            _gateDirection = newDirection;
            transform.rotation = newDirection.DirToZRot();
        }
        
        public void Unlock()
        {
            _locked = false;
            gate.SetActive(false);
        }

        public bool IsBlockingWay(Vector2 direction)
        {
            return direction == _gateDirection && _locked;
        }
    }
}
