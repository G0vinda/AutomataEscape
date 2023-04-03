using System;
using System.Collections.Generic;
using System.Linq;
using Helper;
using Tiles;
using UI;
using UI.Transition;
using UnityEngine;

public class LevelGridManager : MonoBehaviour
{
    [SerializeField] private Vector2 gridStartPosition;
    [SerializeField] private GameObject keyPrefab;
    
    [Header("TilePrefabs")]
    [SerializeField] private GameObject floorTilePrefab;
    [SerializeField] private GameObject goalTilePrefab;
    [SerializeField] private GameObject orangeTilePrefab;
    [SerializeField] private GameObject purpleTilePrefab;
    [SerializeField] private GameObject blueGateTilePrefab;
    [SerializeField] private GameObject redGateTilePrefab;

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
                
                InstantiateTile(tileToInstantiate, placementPosition, placementDirection);
            }
        }
        
        CreateWalls(gridSource.GetLength(1), gridSource.GetLength(0));
    }

    public static int GetSpriteSortingOrderFromCoordinates(Vector2Int coordinates)
    {
        return -coordinates.y * 2;
    }

    private static void AdjustWallSpriteLayer(GameObject wall, Vector2Int coordinates, int modifier = 0)
    {
        wall.GetComponentInChildren<SpriteRenderer>().sortingOrder = -coordinates.y * 2 + modifier;
    }

    public Vector2 CoordinatesToPosition(Vector2Int coordinates)
    {
        return coordinates * _tileSize;
    }

    private void InstantiateTile(GameObject tilePrefab, Vector2Int placementCoordinates, Direction placementDirection)
    {
        var positionOffset = placementCoordinates * _tileSize; 
        var newTile = Instantiate(tilePrefab, gridStartPosition + positionOffset, Quaternion.identity, transform);
        newTile.name = $"Tile_{placementCoordinates.x}_{placementCoordinates.y}";
        if (newTile.TryGetComponent<GateTile>(out var newGateTile))
            newGateTile.Initialize(placementDirection, -placementCoordinates.y * 2);
        
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
                    CreateWallOnEmptyTile(currentCoordinates);
                }
            }
        }
        
        // Create walls on top border
        var borderCoordinates = new Vector2Int(0, 0);
        for (var x = 0; x < xMax; x++)
        {
            borderCoordinates.x = x;
            if(!Grid.ContainsKey(borderCoordinates))
                continue;
            
            if (x == xMax - 1)
            {
                CreateRightWall(borderCoordinates);
                if (!Grid.ContainsKey(borderCoordinates + Vector2Int.down))
                    CreateLowerConnectorWall(borderCoordinates, true);
            }

            CreateUpperWall(borderCoordinates);
            if (!Grid.ContainsKey(borderCoordinates + Vector2Int.right))
                CreateUpperConnectorWall(borderCoordinates, true);
            
            if (!Grid.ContainsKey(borderCoordinates + Vector2Int.left))
                CreateUpperConnectorWall(borderCoordinates, false);
        }

        // Create walls on right border
        if (yMax == 1)
        {
            borderCoordinates = new Vector2Int(xMax - 1, 0);
            if (Grid.ContainsKey(borderCoordinates))
            {
                CreateBottomWall(borderCoordinates);   
            }
        }
        else
        {
            borderCoordinates = new Vector2Int(xMax - 1, -1);
        }
        for (var y = -1; y > -yMax; y--)
        {
            borderCoordinates.y = y;
            if (!Grid.ContainsKey(borderCoordinates))
            {
                continue;   
            }

            if (y == -(yMax - 1))
            {
                CreateBottomWall(borderCoordinates);
            }

            if (!Grid.ContainsKey(borderCoordinates + Vector2Int.down))
                CreateLowerConnectorWall(borderCoordinates, true);

            CreateRightWall(borderCoordinates);
        }
        
        // Create walls on bottom border
        borderCoordinates = new Vector2Int(xMax - 2, -(yMax - 1));
        for (var x = xMax - 2; x >= 0; x--)
        {
            borderCoordinates.x = x;
            if(!Grid.ContainsKey(borderCoordinates))
                continue;
            
            if (x == 0)
            {
                CreateLeftWall(borderCoordinates);
                CreateLowerConnectorWall(borderCoordinates, false);
                if (!Grid.ContainsKey(borderCoordinates + Vector2Int.up))
                {
                    CreateUpperConnectorWall(borderCoordinates, false);
                }
            }

            CreateBottomWall(borderCoordinates);
        }
        
        // Create walls on left border
        borderCoordinates = new Vector2Int(0, -yMax + 2);
        for (var y = -yMax + 2; y <= 0; y++)
        {
            borderCoordinates.y = y;
            if (!Grid.ContainsKey(borderCoordinates))
            {
                continue;   
            }

            CreateLeftWall(borderCoordinates);
            
            if (!Grid.ContainsKey(borderCoordinates + Vector2Int.down))
                CreateLowerConnectorWall(borderCoordinates, false);
        }

    }

    private void CreateWallOnEmptyTile(Vector2Int coordinates)
    {
        var upperTileExists = Grid.ContainsKey(coordinates + Vector2Int.up);
        var rightTileExists = Grid.ContainsKey(coordinates + Vector2Int.right);
        var lowerTileExists = Grid.ContainsKey(coordinates + Vector2Int.down);
        var leftTileExists = Grid.ContainsKey(coordinates + Vector2Int.left);

        if (upperTileExists && rightTileExists)
        {
            if (lowerTileExists)
            {
                if (leftTileExists)
                {
                    // Create wall block
                    CreateComplexWall(wallBlockPrefab, coordinates);
                }
                else
                {
                    // Create right facing U wall
                    CreateComplexWall(sideFacingUWallPrefab, coordinates);
                    var lowerLeftCoordinates = coordinates + new Vector2Int(-1, -1);
                    if(!Grid.ContainsKey(lowerLeftCoordinates))
                        CreateUpperConnectorWall(coordinates + Vector2Int.down, false);
                }
            }
            else if (leftTileExists)
            {
                // Create up facing U wall
                CreateComplexWall(upFacingUWallPrefab, coordinates);
                
                var lowerRightCoordinates = coordinates + new Vector2Int(1, -1);
                if(!Grid.ContainsKey(lowerRightCoordinates))
                    CreateLowerConnectorWall(coordinates + Vector2Int.right, false);

                var lowerLeftCoordinates = coordinates + new Vector2Int(-1, -1);
                if(!Grid.ContainsKey(lowerLeftCoordinates))
                    CreateLowerConnectorWall(coordinates + Vector2Int.left, true);
            }
            else
            {
                // Create up right corner wall
                CreateComplexWall(upperCornerPrefab, coordinates);
                var lowerRightCoordinates = coordinates + new Vector2Int(1, -1);
                if(!Grid.ContainsKey(lowerRightCoordinates))
                    CreateLowerConnectorWall(coordinates + Vector2Int.right, false);
            }
            
            return;
        }

        if (upperTileExists && leftTileExists)
        {
            if (lowerTileExists)
            {
                // Create left facing U wall
                CreateComplexWall(sideFacingUWallPrefab, coordinates, false);
                var lowerRightCoordinates = coordinates + new Vector2Int(1, -1);
                if(!Grid.ContainsKey(lowerRightCoordinates))
                    CreateUpperConnectorWall(coordinates + Vector2Int.down, true);
            }
            else
            {
                // Create up left corner wall                
                CreateComplexWall(upperCornerPrefab, coordinates, false);
                var lowerLeftCoordinates = coordinates + new Vector2Int(-1, -1);
                if(!Grid.ContainsKey(lowerLeftCoordinates))
                    CreateLowerConnectorWall(coordinates + Vector2Int.left, true);
            }

            return;
        }

        if (lowerTileExists && rightTileExists)
        {
            if (leftTileExists)
            {
                // Create down facing U wall
                CreateComplexWall(downFacingUWallPrefab, coordinates);
                var upperRightCoordinates = coordinates + new Vector2Int(1, 1);
                if(!Grid.ContainsKey(upperRightCoordinates))
                    CreateUpperConnectorWall(coordinates + Vector2Int.right, false);
            }
            else
            {
                // Create down right corner wall
                CreateComplexWall(lowerCornerPrefab, coordinates);
                var lowerLeftCoordinates = coordinates + new Vector2Int(-1, -1);
                if(!Grid.ContainsKey(lowerLeftCoordinates))
                    CreateUpperConnectorWall(coordinates + Vector2Int.down, false);
            }

            return;
        }

        if (lowerTileExists && leftTileExists)
        {
            // Create down left corner wall
            CreateComplexWall(lowerCornerPrefab, coordinates, false);
            var lowerRightCoordinates = coordinates + new Vector2Int(1, -1);
            if(!Grid.ContainsKey(lowerRightCoordinates))
                CreateUpperConnectorWall(coordinates + Vector2Int.down, true);
            
            return;
        }

        if (upperTileExists)
        {
            var upperCoordinates = coordinates + Vector2Int.up;
            
            // Create upper wall 
            CreateBottomWall(upperCoordinates);
        }

        if (lowerTileExists)
        {
            var lowerCoordinates = coordinates + Vector2Int.down;
            
            // Create lower wall 
            CreateUpperWall(lowerCoordinates);
            if(!Grid.ContainsKey(lowerCoordinates + Vector2Int.left))
                CreateUpperConnectorWall(lowerCoordinates, false);
            
            if(!Grid.ContainsKey(lowerCoordinates + Vector2Int.right))
                CreateUpperConnectorWall(lowerCoordinates, true);
        }
        
        if (rightTileExists)
        {
            var rightCoordinates = coordinates + Vector2Int.right;

            // Create right wall
            CreateLeftWall(rightCoordinates);
            if(!Grid.ContainsKey(rightCoordinates + Vector2Int.down))
                CreateLowerConnectorWall(rightCoordinates, false);
        }

        if (leftTileExists)
        {
            var leftCoordinates = coordinates + Vector2Int.left;
            
            // Create left wall
            CreateRightWall(leftCoordinates);
            if(!Grid.ContainsKey(leftCoordinates + Vector2Int.down))
                CreateLowerConnectorWall(leftCoordinates, true);
        }
    }
    
    private void CreateUpperWall(Vector2Int coordinates)
    {
        var topWallPosition = CoordinatesToPosition(coordinates) + _tileSize * Vector2.up * 0.5f;
        var newWall = InstantiateWall(solidWallPrefab, topWallPosition);
        AdjustWallSpriteLayer(newWall, coordinates, -1);
    }
    
    private void CreateRightWall(Vector2Int coordinates)
    {
        var rightWallPosition = CoordinatesToPosition(coordinates) + _tileSize * Vector2.right * 0.5f;
        var newWall = InstantiateWall(rightWallPrefab, rightWallPosition);
        AdjustWallSpriteLayer(newWall, coordinates);
    }
    
    private void CreateBottomWall(Vector2Int coordinates)
    {
        var bottomWallPosition = CoordinatesToPosition(coordinates) + _tileSize * Vector2.down * 0.5f;
        var newWall = InstantiateWall(transparentWallPrefab, bottomWallPosition);
        AdjustWallSpriteLayer(newWall, coordinates, 1);
    }
    
    private void CreateLeftWall(Vector2Int coordinates)
    {
        var leftWallPosition = CoordinatesToPosition(coordinates) + _tileSize * Vector2.left * 0.5f;
        var newWall = InstantiateWall(leftWallPrefab, leftWallPosition);
        AdjustWallSpriteLayer(newWall, coordinates);
    }

    private void CreateComplexWall(GameObject wallPrefab, Vector2Int coordinates, bool facesRight = true)
    {
        var wallPosition = CoordinatesToPosition(coordinates);
        var newWall = InstantiateWall(wallPrefab, wallPosition);
        AdjustWallSpriteLayer(newWall, coordinates);
        
        if (!facesRight)
            newWall.GetComponentInChildren<SpriteRenderer>().flipX = true;
    }

    private void CreateUpperConnectorWall(Vector2Int coordinates, bool facesRight)
    {
        if (facesRight)
        {
            var connectorPosition = CoordinatesToPosition(coordinates) + new Vector2(_tileSize.x, _tileSize.y) * 0.5f;
            var newWallConnector = InstantiateWall(solidWallConnectorRightPrefab, connectorPosition);
            AdjustWallSpriteLayer(newWallConnector, coordinates, -1);
        }
        else
        {
            var connectorPosition = CoordinatesToPosition(coordinates) + new Vector2(-_tileSize.x, _tileSize.y) * 0.5f;
            var newWallConnector = InstantiateWall(solidWallConnectorLeftPrefab, connectorPosition);
            AdjustWallSpriteLayer(newWallConnector, coordinates, -1);
        }
    }

    private void CreateLowerConnectorWall(Vector2Int coordinates, bool facesRight)
    {
        if (facesRight)
        {
            var connectorPosition = CoordinatesToPosition(coordinates) + new Vector2(_tileSize.x, -_tileSize.y) * 0.5f;
            var newWallConnector = InstantiateWall(transparentWallConnectorRightPrefab, connectorPosition);
            
            AdjustWallSpriteLayer(newWallConnector, coordinates, 1);
        }
        else
        {
            var connectorPosition = CoordinatesToPosition(coordinates) + new Vector2(-_tileSize.x, -_tileSize.y) * 0.5f;
            var newWallConnector = InstantiateWall(transparentWallConnectorLeftPrefab, connectorPosition);
            
            AdjustWallSpriteLayer(newWallConnector, coordinates, 1);
        }
    }

    public bool UnlockGateWithKeyIfPossible(Vector2Int coordinates, KeyType keyType)
    {
        if (Grid[coordinates].TryGetComponent<GateTile>(out var gateTile))
        {
            return gateTile.Unlock(keyType);
        }

        return false;
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
        BlueGateRight,
        BlueGateLeft,
        RedGateRight,
        RedGateLeft,
    }
    
    public enum KeyType
    {
        None,
        Blue,
        Red
    }
}
