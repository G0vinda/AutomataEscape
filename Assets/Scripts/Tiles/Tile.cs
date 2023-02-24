using UnityEngine;

namespace Tiles
{
    public class Tile : MonoBehaviour
    {
        public enum TileType
        {
            None,
            Floor,
            Goal,
            GateUp,
            GateDown,
            GateRight,
            GateLeft
        }

        [SerializeField] private TileType type;

        public TileType GetTileType()
        {
            return type;
        }
    }
}
