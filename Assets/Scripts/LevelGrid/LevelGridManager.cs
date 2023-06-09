using System;
using System.Collections.Generic;
using Helper;
using UI.Transition;
using UnityEngine;
using UnityEngine.WSA;

namespace LevelGrid
{
    public class LevelGridManager : MonoBehaviour
    {
        [Header("TilePrefabs")] 
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject goalTilePrefab;
        [SerializeField] private GameObject orangeTilePrefab;
        [SerializeField] private GameObject purpleTilePrefab;
        [SerializeField] private GameObject blueGateTilePrefab;
        [SerializeField] private GameObject redGateTilePrefab;
        [SerializeField] private GameObject portalTilePrefab;
        
        public Dictionary<Vector2Int, GameObject> Grid { get; } = new();

        private readonly Vector2 _gridStartPosition = Vector2.zero;
        private Vector2 _tileSize;
        private WallBuilder _wallBuilder;

        private void Awake()
        {
            _tileSize = tilePrefab
                .GetComponentInChildren<SpriteRenderer>().bounds.size;
            _wallBuilder = GetComponent<WallBuilder>();
            _wallBuilder.Initialize(_tileSize);
        }

        public void CreateLevelGrid(TileType[,] gridSource)
        {
            ClearGrid();

            for (var y = 0; y > -gridSource.GetLength(0); y--)
            {
                for (var x = 0; x < gridSource.GetLength(1); x++)
                {
                    GameObject tileToInstantiate;
                    var placementPosition = new Vector2Int(x, y);
                    var placementDirection = Direction.Up;
                    switch (gridSource[-y, x])
                    {
                        case TileType.None:
                            continue;
                        case TileType.Floor:
                            tileToInstantiate = tilePrefab;
                            break;
                        case TileType.Goal:
                            tileToInstantiate = goalTilePrefab;
                            break;
                        case TileType.Orange:
                            tileToInstantiate = orangeTilePrefab;
                            break;
                        case TileType.Purple:
                            tileToInstantiate = purpleTilePrefab;
                            break;
                        case TileType.Portal:
                            tileToInstantiate = portalTilePrefab;
                            break;
                        case TileType.BlueGateRight:
                            tileToInstantiate = blueGateTilePrefab;
                            placementDirection = Direction.Right;
                            break;
                        case TileType.BlueGateLeft:
                            tileToInstantiate = blueGateTilePrefab;
                            placementDirection = Direction.Left;
                            break;
                        case TileType.RedGateRight:
                            tileToInstantiate = redGateTilePrefab;
                            placementDirection = Direction.Right;
                            break;
                        case TileType.RedGateLeft:
                            tileToInstantiate = redGateTilePrefab;
                            placementDirection = Direction.Left;
                            break;
                        default:
                            throw new ArgumentException();
                    }

                    InstantiateTile(tileToInstantiate, placementPosition,
                        placementDirection);
                }
            }

            _wallBuilder.CreateWalls(gridSource.GetLength(1), gridSource.GetLength(0), Grid);
        }

        public static int GetSpriteSortingOrderFromCoordinates(
            Vector2Int coordinates)
        {
            return -coordinates.y * 2;
        }

        private void InstantiateTile(GameObject tilePrefab,
            Vector2Int placementCoordinates, Direction placementDirection)
        {
            var positionOffset = placementCoordinates * _tileSize;
            var newTile = Instantiate(tilePrefab,
                _gridStartPosition + positionOffset, Quaternion.identity,
                transform);
            newTile.name =
                $"Tile_{placementCoordinates.x}_{placementCoordinates.y}";
            if (newTile.TryGetComponent<GateTile>(out var newGateTile))
                newGateTile.Initialize(placementDirection,
                    -placementCoordinates.y * 2);

            Grid.Add(placementCoordinates, newTile);
        }
        
        public bool UnlockGateWithKeyIfPossible(Vector2Int coordinates,
            KeyType keyType)
        {
            if (Grid[coordinates]
                .TryGetComponent<GateTile>(out var gateTile))
            {
                return gateTile.Unlock(keyType);
            }

            return false;
        }

        public bool CheckIfWayIsBlocked(Vector2Int currentCoordinates,
            Direction moveDirection)
        {
            var nextCoordinates =
                currentCoordinates + moveDirection.ToVector2Int();
            if (!Grid.ContainsKey(nextCoordinates))
                return true;

            var currentTile = Grid[currentCoordinates];

            return currentTile.TryGetComponent<GateTile>(out var gateTile) &&
                   gateTile.IsBlockingWay(moveDirection);
        }

        public bool CheckIfTileIsPortal(Vector2Int tileCoordinates)
        {
            return CheckIfTileHasTag(tileCoordinates, "Portal");
        }

        public bool CheckIfTileIsGoal(Vector2Int tileCoordinates)
        {
            return CheckIfTileHasTag(tileCoordinates, "Goal");
        }

        public bool CheckIfTileIsOrange(Vector2Int tileCoordinates)
        {
            return CheckIfTileHasTag(tileCoordinates, "OrangeTile");
        }

        public bool CheckIfTileIsPurple(Vector2Int tileCoordinates)
        {
            return CheckIfTileHasTag(tileCoordinates, "PurpleTile");
        }

        private bool CheckIfTileHasTag(Vector2Int tileCoordinates,
            string tag)
        {
            return Grid[tileCoordinates].CompareTag(tag);
        }

        public Vector2 GetTilePosition(Vector2Int tileCoordinates)
        {
            return Grid[tileCoordinates].transform.position;
        }

        public List<SpriteRenderer> GetTileSpriteRenderers()
        {
            var renderers = new List<SpriteRenderer>();
            foreach (var tile in Grid)
            {
                renderers.Add(tile.Value
                    .GetComponentInChildren<SpriteRenderer>());
            }

            return renderers;
        }

        private void ClearGrid()
        {
            foreach (var tileEntry in Grid)
            {
                Destroy(tileEntry.Value.gameObject);
            }
            
            _wallBuilder.DestroyWalls();
            Grid.Clear();
        }

        public enum TileType
        {
            None,
            Floor,
            Goal,
            Orange,
            Purple,
            BlueGateRight,
            BlueGateLeft,
            RedGateRight,
            RedGateLeft,
            Portal
        }

        public enum KeyType
        {
            None,
            Blue,
            Red
        }
    }
}