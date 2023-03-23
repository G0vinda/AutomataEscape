using Helper;
using UI;
using UnityEngine;

namespace Tiles
{
    public class GateTile : MonoBehaviour
    {
        [SerializeField] private GameObject gate;

        private Direction _gateDirection;
        private bool _locked = true;

        public void SetDirection(Direction newDirection)
        {
            _gateDirection = newDirection;
        }
        
        public void Unlock()
        {
            _locked = false;
            gate.SetActive(false);
        }

        public bool IsBlockingWay(Direction direction)
        {
            return direction == _gateDirection && _locked;
        }
    }
}
