using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tiles;
using Unity.Mathematics;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Vector3 gridStartPosition;
    [SerializeField] private Tile floorTilePrefab;
    [SerializeField] private Tile goalTilePrefab;
    [SerializeField] private Tile gateTilePrefab;
    [SerializeField] private GameObject keyPrefab;

    public Dictionary<(int, int), Tile> Grid => _grid;

    private Dictionary<(int, int), Tile> _grid = new ();

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
                        _grid.Add((x,y), InstantiateGateTile(gridSource[-y, x], x, y));
                        continue;
                    default:
                        Debug.LogError("Invalid tileType on level creation!");
                        return;
                }
                
                _grid.Add((x,y), InstantiateTile(tileType, x, y));
            }
        }
    }

    private Tile InstantiateGateTile(Tile.TileType type, int x, int y)
    {
        var newGateTile = (GateTile)InstantiateTile(gateTilePrefab, x, y);
        switch (type)
        {
            case Tile.TileType.GateUp:
                newGateTile.SetDirection(Vector2.up);
                break;
            case Tile.TileType.GateDown:
                newGateTile.SetDirection(Vector2.down);
                break;
            case Tile.TileType.GateRight:
                newGateTile.SetDirection(Vector2.right);
                break;
            case Tile.TileType.GateLeft:
                newGateTile.SetDirection(Vector2.left);
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

    public void DropKey((int, int) dropCoordinates)
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

    public bool CheckIfWayIsBlocked((int, int) currentCoordinates, Vector2 moveDirection)
    {
        var nextCoordinates = (currentCoordinates.Item1 + (int)moveDirection.x, currentCoordinates.Item2 + (int)moveDirection.y);
        if (!_grid.ContainsKey(nextCoordinates))
            return true;

        var currentTile = _grid[currentCoordinates];
        if (currentTile is GateTile && ((GateTile)currentTile).IsBlockingWay(moveDirection))
            return true;

        return false;
    }

    public bool CheckIfTileIsGoal((int, int) tileCoordinates)
    {
        return _grid[tileCoordinates].GetTileType() == Tile.TileType.Goal;
    }

    public Vector3 GetTilePosition((int, int) tileCoordinates)
    {
        return _grid[tileCoordinates].transform.position;
    }

    private void ClearGrid()
    {
        foreach (var tileEntry in _grid)
        {
            Destroy(tileEntry.Value);
        }
        _grid.Clear();
    }
}
