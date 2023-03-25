using System;
using System.Collections.Generic;
using Helper;
using Tiles;
using UI;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Vector2 gridStartPosition;
    [SerializeField] private GameObject floorTilePrefab;
    [SerializeField] private GameObject goalTilePrefab;
    [SerializeField] private GameObject orangeTilePrefab;
    [SerializeField] private GameObject purpleTilePrefab;
    [SerializeField] private GameObject gateTilePrefab;

    [SerializeField] private GameObject upWallPrefab;
    [SerializeField] private GameObject sideWallPrefab;

    [SerializeField] private GameObject keyPrefab;

    public Dictionary<Vector2Int, GameObject> Grid { get; } = new ();

    private Vector2 _tileSize;

    private void Awake()
    {
        _tileSize = floorTilePrefab.GetComponentInChildren<SpriteRenderer>().bounds.size;
        Debug.Log($"TileSize is {_tileSize}");
    }

    public void CreateLevelBasedOnGrid(TileType[,] gridSource)
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
                        tileToInstantiate = floorTilePrefab;
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
                    case TileType.GateUp:
                        tileToInstantiate = gateTilePrefab;
                        break;
                    case TileType.GateRight:
                        placementDirection = Direction.Right;
                        tileToInstantiate = gateTilePrefab;
                        break;
                    case TileType.GateDown:
                        placementDirection = Direction.Down;
                        tileToInstantiate = gateTilePrefab;
                        break;
                    case TileType.GateLeft:
                        placementDirection = Direction.Left;
                        tileToInstantiate = gateTilePrefab;
                        break;
                    default:
                        Debug.LogError("Invalid tileType on level creation!");
                        return;
                }
                
                InstantiateTile(tileToInstantiate, placementPosition, placementDirection);
                var wallsNeeded = new WallsNeeded(
                    y == 0, // Needs wall on top
                    x == gridSource.GetLength(1) - 1, // Needs wall on right
                    y == gridSource.GetLength(0) - 1, // Needs wall on bottom
                    x == 0); // Needs wall on left
                
                InstantiateWallIfNeeded(placementPosition, wallsNeeded);
            }
        }
    }

    private void InstantiateTile(GameObject tilePrefab, Vector2Int placementCoordinates, Direction placementDirection)
    {
        var positionOffset = (Vector2)placementCoordinates * _tileSize; 
        var newTile = Instantiate(tilePrefab, gridStartPosition + positionOffset, placementDirection.ToZRotation(),
            transform);
        newTile.name = $"Tile_{placementCoordinates.x}_{placementCoordinates.y}";
        if (newTile.TryGetComponent<GateTile>(out var newGateTile))
            newGateTile.SetDirection(placementDirection);
        
        Grid.Add(placementCoordinates, newTile);
    }

    private void InstantiateWallIfNeeded(Vector2Int tileCoordinates, WallsNeeded wallsNeeded)
    {
        var tilePosition = (Vector2)tileCoordinates * _tileSize + gridStartPosition;
        var wallPositionOffset = new Vector2();

        if (wallsNeeded.OnTop) // Create wall on top
        {
            wallPositionOffset = Vector2.up * _tileSize * 0.5f;
            Instantiate(upWallPrefab, tilePosition + wallPositionOffset, Quaternion.identity);
        }

        if (wallsNeeded.OnLeft)
        {
            wallPositionOffset = Vector2.left * _tileSize * 0.5f;
            Instantiate(sideWallPrefab, tilePosition + wallPositionOffset, Quaternion.identity);
        }
    }

    public void DropKey(Vector2Int dropCoordinates)
    {
        var tile = Grid[dropCoordinates];
        if (tile.TryGetComponent<GateTile>(out var gateTile))
        {
            gateTile.Unlock();
        }
        else
        {
            Instantiate(keyPrefab, GetTilePosition(dropCoordinates), Quaternion.identity, transform);   
        }
    }

    public bool CheckIfWayIsBlocked(Vector2Int currentCoordinates, Direction moveDirection)
    {
        var nextCoordinates = currentCoordinates + moveDirection.ToVector2Int();
        if (!Grid.ContainsKey(nextCoordinates))
            return true;

        var currentTile = Grid[currentCoordinates];

        return currentTile.TryGetComponent<GateTile>(out var gateTile) && gateTile.IsBlockingWay(moveDirection);
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

    private bool CheckIfTileHasTag(Vector2Int tileCoordinates, string checkedTag)
    {
        return Grid[tileCoordinates].CompareTag(checkedTag);
    }

    public Vector3 GetTilePosition(Vector2Int tileCoordinates)
    {
        return Grid[tileCoordinates].transform.position;
    }

    public List<SpriteRenderer> GetTileObjectRenderers()
    {
        var renderers = new List<SpriteRenderer>();
        foreach (var tile in Grid)
        {
            renderers.Add(tile.Value.GetComponentInChildren<SpriteRenderer>());
        }
        
        return renderers;
    }

    private void ClearGrid()
    {
        foreach (var tileEntry in Grid)
        {
            Destroy(tileEntry.Value.gameObject);
        }
        Grid.Clear();
    }

    public enum TileType
    {
        None,
        Floor,
        Goal,
        Orange,
        Purple,
        GateUp,
        GateDown,
        GateRight,
        GateLeft,
    }

    private struct WallsNeeded
    {
        public bool OnTop { get; }
        public bool OnRight { get; }
        public bool OnBottom { get; }
        public bool OnLeft { get; }

        public WallsNeeded(bool top, bool right, bool bottom, bool left)
        {
            OnTop = top;
            OnRight = right;
            OnBottom = bottom;
            OnLeft = left;
        }
    }
}
