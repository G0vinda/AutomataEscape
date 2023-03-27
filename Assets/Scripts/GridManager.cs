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
    [SerializeField] private GameObject keyPrefab;
    
    [Header("TilePrefabs")]
    [SerializeField] private GameObject floorTilePrefab;
    [SerializeField] private GameObject goalTilePrefab;
    [SerializeField] private GameObject orangeTilePrefab;
    [SerializeField] private GameObject purpleTilePrefab;
    [SerializeField] private GameObject gateTilePrefab;

    [Header("WallPrefabs")]
    [SerializeField] private GameObject solidWallPrefab;
    [SerializeField] private GameObject transparentWallPrefab;
    [SerializeField] private GameObject rightWallPrefab;
    [SerializeField] private GameObject leftWallPrefab;
    [SerializeField] private GameObject downFacingUWallPrefab;
    [SerializeField] private GameObject sideFacingUWallPrefab;
    [SerializeField] private GameObject upFacingUWallPrefab;
    [SerializeField] private GameObject wallBlockPrefab;
    [SerializeField] private GameObject upperCornerPrefab;
    [SerializeField] private GameObject lowerCornerPrefab;
    [SerializeField] private GameObject solidWallConnectorRightPrefab;
    [SerializeField] private GameObject solidWallConnectorLeftPrefab;
    [SerializeField] private GameObject transparentWallConnectorRightPrefab;
    [SerializeField] private GameObject transparentWallConnectorLeftPrefab;

    public Dictionary<Vector2Int, GameObject> Grid { get; } = new ();

    private Vector2 _tileSize;
    private List<GameObject> _placedWalls = new ();

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
            }
        }
        
        CreateWalls(gridSource.GetLength(1), gridSource.GetLength(0));
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

    private GameObject InstantiateWall(GameObject wallPrefab, Vector2 position)
    {
        var newWall = Instantiate(wallPrefab, position, Quaternion.identity);
        _placedWalls.Add(newWall);
        return newWall;
    }

    private void CreateWalls(int xMax, int yMax)
    {
        for (var x = 0; x < xMax; x++)
        {
            for (var y = -(yMax - 1); y <= 0; y++)
            {
                var currentCoordinates = new Vector2Int(x, y);
                if (!Grid.ContainsKey(currentCoordinates))
                {
                    CreateWallsOnEmptyTile(currentCoordinates);
                }
            }
        }
        
        // Create walls on top border
        var createLeftWallConnector = true;
        var borderCoordinates = new Vector2Int(0, 0);
        for (var x = 0; x < xMax; x++)
        {
            borderCoordinates.x = x;
            if(!Grid.ContainsKey(borderCoordinates))
                continue;

            if (x == 0)
            {
                CreateLeftWall(borderCoordinates);
            }else if (x == xMax - 1)
            {
                CreateRightWall(borderCoordinates);
            }

            if (createLeftWallConnector)
            {
                createLeftWallConnector = false;

                var connectorPosition = (Vector2)Grid[borderCoordinates].transform.position + new Vector2(-_tileSize.x, _tileSize.y) * 0.5f;
                Instantiate(solidWallConnectorLeftPrefab, connectorPosition, Quaternion.identity);
            }
            
            if (!Grid.ContainsKey(borderCoordinates + Vector2Int.right))
            {
                createLeftWallConnector = true;

                var connectorPosition = (Vector2)Grid[borderCoordinates].transform.position + new Vector2(_tileSize.x, _tileSize.y) * 0.5f;
                Instantiate(solidWallConnectorRightPrefab, connectorPosition, Quaternion.identity);
            }

            CreateUpperWall(borderCoordinates);
        }

        // Create walls on right border
        var createUpperWallConnector = false;
        borderCoordinates = new Vector2Int(xMax - 1, -1);
        for (var y = -1; y > -yMax; y--)
        {
            borderCoordinates.y = y;
            if(!Grid.ContainsKey(borderCoordinates))
                continue;
            
            if (y == -(yMax - 1))
            {
                CreateBottomWall(borderCoordinates);
            }
            
            if (createUpperWallConnector)
            {
                createUpperWallConnector = false;

                var connectorPosition = (Vector2)Grid[borderCoordinates].transform.position + new Vector2(_tileSize.x, _tileSize.y) * 0.5f;
                Instantiate(solidWallConnectorRightPrefab, connectorPosition, Quaternion.identity);
            }
            
            if (!Grid.ContainsKey(borderCoordinates + Vector2Int.down))
            {
                createUpperWallConnector = true;

                var connectorPosition = (Vector2)Grid[borderCoordinates].transform.position + new Vector2(_tileSize.x, -_tileSize.y) * 0.5f;
                Instantiate(transparentWallConnectorRightPrefab, connectorPosition, Quaternion.identity);
            }
            
            CreateRightWall(borderCoordinates);
        }
        
        // Create walls on bottom border
        var createRightWallConnector = false;
        borderCoordinates = new Vector2Int(xMax - 2, -(yMax - 1));
        for (var x = xMax - 2; x >= 0; x--)
        {
            borderCoordinates.x = x;
            if(!Grid.ContainsKey(borderCoordinates))
                continue;
            
            if (x == 0)
            {
                CreateLeftWall(borderCoordinates);
            }
            
            if (createRightWallConnector)
            {
                createRightWallConnector = false;

                var connectorPosition = (Vector2)Grid[borderCoordinates].transform.position + new Vector2(_tileSize.x, _tileSize.y) * 0.5f;
                Instantiate(transparentWallConnectorRightPrefab, connectorPosition, Quaternion.identity);
            }
            
            if (!Grid.ContainsKey(borderCoordinates + Vector2Int.left))
            {
                createRightWallConnector = true;

                var connectorPosition = (Vector2)Grid[borderCoordinates].transform.position + new Vector2(-_tileSize.x, -_tileSize.y) * 0.5f;
                Instantiate(transparentWallConnectorLeftPrefab, connectorPosition, Quaternion.identity);
            }
            
            CreateBottomWall(borderCoordinates);
        }
        
        // Create walls on left border
        var createLowerWallConnector = false;
        borderCoordinates = new Vector2Int(0, -yMax + 2);
        for (var y = -yMax + 2; y < 0; y++)
        {
            borderCoordinates.y = y;
            if(!Grid.ContainsKey(borderCoordinates))
                continue;
            
            if (createLowerWallConnector)
            {
                createLowerWallConnector = false;

                var connectorPosition = (Vector2)Grid[borderCoordinates].transform.position + new Vector2(-_tileSize.x, -_tileSize.y) * 0.5f;
                Instantiate(transparentWallConnectorLeftPrefab, connectorPosition, Quaternion.identity);
            }
            
            if (!Grid.ContainsKey(borderCoordinates + Vector2Int.up))
            {
                createLowerWallConnector = true;

                var connectorPosition = (Vector2)Grid[borderCoordinates].transform.position + new Vector2(-_tileSize.x, _tileSize.y) * 0.5f;
                Instantiate(solidWallConnectorLeftPrefab, connectorPosition, Quaternion.identity);
            }
            
            CreateLeftWall(borderCoordinates);
        }

    }

    private void CreateWallsOnEmptyTile(Vector2Int coordinates)
    {
        var upperTileExists = Grid.ContainsKey(coordinates + Vector2Int.up);
        var upperTile = upperTileExists ? Grid[coordinates + Vector2Int.up] : null;
        
        var rightTileExists = Grid.ContainsKey(coordinates + Vector2Int.right);
        var rightTile = rightTileExists ? Grid[coordinates + Vector2Int.right] : null;
        
        var lowerTileExists = Grid.ContainsKey(coordinates + Vector2Int.down);
        var lowerTile = lowerTileExists ? Grid[coordinates + Vector2Int.down] : null;
        
        var leftTileExists = Grid.ContainsKey(coordinates + Vector2Int.left);
        var leftTile = leftTileExists ? Grid[coordinates + Vector2Int.left] : null;

        if (upperTileExists && rightTileExists)
        {
            var placementPosition = (Vector2)upperTile.transform.position + Vector2.down * _tileSize;
            if (lowerTileExists)
            {
                if (leftTileExists)
                {
                    // Create wall block
                    InstantiateWall(wallBlockPrefab, placementPosition);
                }
                else
                {
                    // Create right facing U wall
                    InstantiateWall(sideFacingUWallPrefab, placementPosition);
                }
            }
            else if (leftTileExists)
            {
                // Create up facing U wall
                InstantiateWall(upFacingUWallPrefab, placementPosition);
            }
            else
            {
                // Create up right corner wall
                InstantiateWall(upperCornerPrefab, placementPosition);
            }
            
            return;
        }

        if (upperTileExists && leftTileExists)
        {
            var placementPosition = (Vector2)upperTile.transform.position + Vector2.down * _tileSize;
            if (lowerTileExists)
            {
                // Create left facing U wall
                var uWallSpriteRenderer = InstantiateWall(sideFacingUWallPrefab, placementPosition).GetComponentInChildren<SpriteRenderer>();
                uWallSpriteRenderer.flipX = true;
            }
            else
            {
                // Create up left corner wall                
                var cornerWallSpriteRenderer = InstantiateWall(upperCornerPrefab, placementPosition).GetComponentInChildren<SpriteRenderer>();
                cornerWallSpriteRenderer.flipX = true;
            }

            return;
        }

        if (lowerTileExists && rightTileExists)
        {
            var placementPosition = (Vector2)lowerTile.transform.position + Vector2.up * _tileSize;
            if (leftTileExists)
            {
                // Create down facing U wall
                InstantiateWall(downFacingUWallPrefab, placementPosition);
            }
            else
            {
                // Create down right corner wall
                InstantiateWall(lowerCornerPrefab, placementPosition);
            }

            return;
        }

        if (lowerTileExists && leftTileExists)
        {
            var placementPosition = (Vector2)lowerTile.transform.position + Vector2.up * _tileSize;
            
            // Create down left corner wall
            var cornerWallSpriteRenderer = InstantiateWall(lowerCornerPrefab, placementPosition).GetComponentInChildren<SpriteRenderer>();
            cornerWallSpriteRenderer.flipX = true;
            return;
        }

        if (upperTileExists)
        {
            var placementPosition = (Vector2)upperTile.transform.position + Vector2.down * _tileSize * 0.5f;
            
            // Create upper wall 
            InstantiateWall(transparentWallPrefab, placementPosition);
        }

        if (lowerTileExists)
        {
            var placementPosition = (Vector2)lowerTile.transform.position + Vector2.up * _tileSize * 0.5f;
            
            // Create upper wall 
            InstantiateWall(solidWallPrefab, placementPosition);
        }
        
        if (rightTileExists)
        {
            var placementPosition = (Vector2)rightTile.transform.position + Vector2.left * _tileSize * 0.5f;

            // Create right wall
            InstantiateWall(rightWallPrefab, placementPosition);
        }

        if (leftTileExists)
        {
            var placementPosition = (Vector2)leftTile.transform.position + Vector2.right * _tileSize * 0.5f;
            
            // Create left wall
            InstantiateWall(leftWallPrefab, placementPosition);
        }
    }
    
    private void CreateUpperWall(Vector2Int coordinates)
    {
        var topWallPosition = (Vector2)Grid[coordinates].transform.position + _tileSize * Vector2.up * 0.5f;
        InstantiateWall(solidWallPrefab, topWallPosition);
    }
    
    private void CreateRightWall(Vector2Int coordinates)
    {
        var rightWallPosition = (Vector2)Grid[coordinates].transform.position +
                                _tileSize * Vector2.right * 0.5f;
        var newWall = InstantiateWall(rightWallPrefab, rightWallPosition);
        newWall.GetComponentInChildren<SpriteRenderer>().sortingOrder = -coordinates.y;
    }
    
    private void CreateBottomWall(Vector2Int coordinates)
    {
        var bottomWallPosition = (Vector2)Grid[coordinates].transform.position +
                                 _tileSize * Vector2.down * 0.5f;
        InstantiateWall(transparentWallPrefab, bottomWallPosition);
    }
    
    private void CreateLeftWall(Vector2Int coordinates)
    {
        var leftWallPosition = (Vector2)Grid[coordinates].transform.position +
                               _tileSize * Vector2.left * 0.5f;
        var newWall = Instantiate(leftWallPrefab, leftWallPosition, Quaternion.identity);
        newWall.GetComponentInChildren<SpriteRenderer>().sortingOrder = -coordinates.y;
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

        foreach (var wall in _placedWalls)
        {
            Destroy(wall);
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
}
