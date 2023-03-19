using System.Collections.Generic;
using Helper;
using Tiles;
using UI;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Vector3 gridStartPosition;
    [SerializeField] private Tile floorTilePrefab;
    [SerializeField] private Tile goalTilePrefab;
    [SerializeField] private Tile gateTilePrefab;
    [SerializeField] private GameObject keyPrefab;

    public Dictionary<Vector2Int, Tile> Grid => _grid;

    private Dictionary<Vector2Int, Tile> _grid = new ();

    public void CreateLevelBasedOnGrid(Tile.TileType[,] gridSource)
    {
        ClearGrid();
        
        for (int y = 0; y > -gridSource.GetLength(0); y--)
        {
            for (int x = 0; x < gridSource.GetLength(1); x++)
            {
                Tile tileType;
                switch (gridSource[-y, x])
                {
                    case Tile.TileType.None:
                        continue;
                    case Tile.TileType.Floor:
                        tileType = floorTilePrefab;
                        break;
                    case Tile.TileType.Goal:
                        tileType = goalTilePrefab;
                        break;
                    case Tile.TileType.GateUp:
                    case Tile.TileType.GateDown:
                    case Tile.TileType.GateRight:
                    case Tile.TileType.GateLeft:
                        _grid.Add(new Vector2Int(x,y), InstantiateGateTile(gridSource[-y, x], x, y));
                        continue;
                    default:
                        Debug.LogError("Invalid tileType on level creation!");
                        return;
                }
                
                _grid.Add(new Vector2Int(x,y), InstantiateTile(tileType, x, y));
            }
        }
    }

    private Tile InstantiateGateTile(Tile.TileType type, int x, int y)
    {
        var newGateTile = (GateTile)InstantiateTile(gateTilePrefab, x, y);
        switch (type)
        {
            case Tile.TileType.GateUp:
                newGateTile.SetDirection(Direction.Up);
                break;
            case Tile.TileType.GateDown:
                newGateTile.SetDirection(Direction.Down);
                break;
            case Tile.TileType.GateRight:
                newGateTile.SetDirection(Direction.Right);
                break;
            case Tile.TileType.GateLeft:
                newGateTile.SetDirection(Direction.Left);
                break;
        }

        return newGateTile;
    }

    private Tile InstantiateTile(Tile tilePrefab, int x, int y)
    {
        var positionOffset = new Vector3(x, y, 0);
        var newTile = Instantiate(tilePrefab, gridStartPosition + positionOffset, Quaternion.identity,
            transform);
        newTile.name = $"Tile_{x}_{y}";

        return newTile;
    }

    public void DropKey(Vector2Int dropCoordinates)
    {
        var tile = _grid[dropCoordinates];
        if (tile is GateTile)
        {
            ((GateTile)tile).Unlock();
        }
        else
        {
            Instantiate(keyPrefab, GetTilePosition(dropCoordinates), Quaternion.identity, transform);   
        }
    }

    public bool CheckIfWayIsBlocked(Vector2Int currentCoordinates, Direction moveDirection)
    {
        var nextCoordinates = currentCoordinates + moveDirection.ToVector2Int();
        if (!_grid.ContainsKey(nextCoordinates))
            return true;

        var currentTile = _grid[currentCoordinates];
        
        return currentTile is GateTile && ((GateTile)currentTile).IsBlockingWay(moveDirection);
    }

    public bool CheckIfTileIsGoal(Vector2Int tileCoordinates)
    {
        return _grid[tileCoordinates].GetTileType() == Tile.TileType.Goal;
    }

    public Vector3 GetTilePosition(Vector2Int tileCoordinates)
    {
        return _grid[tileCoordinates].transform.position;
    }

    public List<SpriteRenderer> GetTileObjectRenderers()
    {
        var renderers = new List<SpriteRenderer>();
        foreach (var tile in _grid)
        {
            renderers.Add(tile.Value.GetComponentInChildren<SpriteRenderer>());
        }

        return renderers;
    }

    private void ClearGrid()
    {
        foreach (var tileEntry in _grid)
        {
            Destroy(tileEntry.Value.gameObject);
        }
        _grid.Clear();
    }
}
