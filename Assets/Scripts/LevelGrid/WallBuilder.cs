using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace LevelGrid
{
    public class WallBuilder : MonoBehaviour
    {
        [Header("WallPrefabs")] 
        [SerializeField] private List<GameObject> solidWallPrefab;
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

        private Vector2 _tileSize;
        private List<GameObject> _placedWalls = new();

        public void Initialize(Vector2 tileSize)
        {
            _tileSize = tileSize;
        }

        public void DestroyWalls()
        {
            _placedWalls.ForEach(Destroy);
        }
        
        private GameObject InstantiateWall(GameObject wallPrefab,
            Vector2 position)
        {
            var newWall =
                Instantiate(wallPrefab, position, Quaternion.identity);
            _placedWalls.Add(newWall);
            return newWall;
        }
        
        private static void AdjustWallSpriteLayer(GameObject wall,
            Vector2Int coordinates, int modifier = 0)
        {
            wall.GetComponentInChildren<SpriteRenderer>().sortingOrder =
                -coordinates.y * 2 + modifier;
        }

        private Vector2 CoordinatesToPosition(Vector2Int coordinates)
        {
            return coordinates * _tileSize;
        }

        public void CreateWalls(int xMax, int yMax, Dictionary<Vector2Int, GameObject> grid)
        {
            for (var x = 0; x < xMax; x++)
            {
                for (var y = -(yMax - 1); y <= 0; y++)
                {
                    var currentCoordinates = new Vector2Int(x, y);
                    if (!grid.ContainsKey(currentCoordinates))
                    {
                        CreateWallOnEmptyTile(currentCoordinates, grid, xMax, yMax);
                    }
                }
            }

            // Create walls on top border
            var borderCoordinates = new Vector2Int(0, 0);
            for (var x = 0; x < xMax; x++)
            {
                borderCoordinates.x = x;
                if (!grid.ContainsKey(borderCoordinates))
                    continue;

                if (x == xMax - 1)
                {
                    CreateRightWall(borderCoordinates);
                    if (!grid.ContainsKey(
                        borderCoordinates + Vector2Int.down))
                        CreateLowerConnectorWall(borderCoordinates, true);
                }

                CreateUpperWall(borderCoordinates, xMax, yMax);
                if (!grid.ContainsKey(borderCoordinates + Vector2Int.right))
                    CreateUpperConnectorWall(borderCoordinates, true);

                if (!grid.ContainsKey(borderCoordinates + Vector2Int.left))
                    CreateUpperConnectorWall(borderCoordinates, false);
            }

            // Create walls on right border
            if (yMax == 1)
            {
                borderCoordinates = new Vector2Int(xMax - 1, 0);
                if (grid.ContainsKey(borderCoordinates))
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
                if (!grid.ContainsKey(borderCoordinates))
                {
                    continue;
                }

                if (y == -(yMax - 1))
                {
                    CreateBottomWall(borderCoordinates);

                    if (borderCoordinates.x == 0)
                    {
                        CreateLeftWall(borderCoordinates);
                        CreateLowerConnectorWall(borderCoordinates, false);
                    }
                }

                if (!grid.ContainsKey(borderCoordinates + Vector2Int.down))
                    CreateLowerConnectorWall(borderCoordinates, true);

                CreateRightWall(borderCoordinates);
            }

            // Create walls on bottom border
            borderCoordinates = new Vector2Int(xMax - 2, -(yMax - 1));
            for (var x = xMax - 2; x >= 0; x--)
            {
                borderCoordinates.x = x;
                if (!grid.ContainsKey(borderCoordinates))
                    continue;

                if (x == 0)
                {
                    CreateLeftWall(borderCoordinates);
                    CreateLowerConnectorWall(borderCoordinates, false);
                    if (!grid.ContainsKey(borderCoordinates + Vector2Int.up))
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
                if (!grid.ContainsKey(borderCoordinates))
                {
                    continue;
                }

                CreateLeftWall(borderCoordinates);

                if (!grid.ContainsKey(borderCoordinates + Vector2Int.down))
                    CreateLowerConnectorWall(borderCoordinates, false);
            }
        }

        private void CreateWallOnEmptyTile(Vector2Int coordinates, Dictionary<Vector2Int, GameObject> grid, int xMax, int yMax)
        {
            var upperTileExists =
                grid.ContainsKey(coordinates + Vector2Int.up);
            var rightTileExists =
                grid.ContainsKey(coordinates + Vector2Int.right);
            var lowerTileExists =
                grid.ContainsKey(coordinates + Vector2Int.down);
            var leftTileExists =
                grid.ContainsKey(coordinates + Vector2Int.left);

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
                        CreateComplexWall(sideFacingUWallPrefab,
                            coordinates);
                        var lowerLeftCoordinates =
                            coordinates + new Vector2Int(-1, -1);
                        if (!grid.ContainsKey(lowerLeftCoordinates))
                            CreateUpperConnectorWall(
                                coordinates + Vector2Int.down, false);
                    }
                }
                else if (leftTileExists)
                {
                    // Create up facing U wall
                    CreateComplexWall(upFacingUWallPrefab, coordinates);

                    var lowerRightCoordinates =
                        coordinates + new Vector2Int(1, -1);
                    if (!grid.ContainsKey(lowerRightCoordinates))
                        CreateLowerConnectorWall(
                            coordinates + Vector2Int.right, false);

                    var lowerLeftCoordinates =
                        coordinates + new Vector2Int(-1, -1);
                    if (!grid.ContainsKey(lowerLeftCoordinates))
                        CreateLowerConnectorWall(
                            coordinates + Vector2Int.left, true);
                }
                else
                {
                    // Create up right corner wall
                    CreateComplexWall(upperCornerPrefab, coordinates);
                    var lowerRightCoordinates =
                        coordinates + new Vector2Int(1, -1);
                    if (!grid.ContainsKey(lowerRightCoordinates))
                        CreateLowerConnectorWall(
                            coordinates + Vector2Int.right, false);
                }

                return;
            }

            if (upperTileExists && leftTileExists)
            {
                if (lowerTileExists)
                {
                    // Create left facing U wall
                    CreateComplexWall(sideFacingUWallPrefab, coordinates,
                        false);
                    var lowerRightCoordinates =
                        coordinates + new Vector2Int(1, -1);
                    if (!grid.ContainsKey(lowerRightCoordinates))
                        CreateUpperConnectorWall(
                            coordinates + Vector2Int.down, true);
                }
                else
                {
                    // Create up left corner wall                
                    CreateComplexWall(upperCornerPrefab, coordinates, false);
                    var lowerLeftCoordinates =
                        coordinates + new Vector2Int(-1, -1);
                    if (!grid.ContainsKey(lowerLeftCoordinates))
                        CreateLowerConnectorWall(
                            coordinates + Vector2Int.left, true);
                }

                return;
            }

            if (lowerTileExists && rightTileExists)
            {
                if (leftTileExists)
                {
                    // Create down facing U wall
                    CreateComplexWall(downFacingUWallPrefab, coordinates);
                    var upperRightCoordinates =
                        coordinates + new Vector2Int(1, 1);
                    if (!grid.ContainsKey(upperRightCoordinates))
                        CreateUpperConnectorWall(
                            coordinates + Vector2Int.right, false);
                }
                else
                {
                    // Create down right corner wall
                    CreateComplexWall(lowerCornerPrefab, coordinates);
                    var lowerLeftCoordinates =
                        coordinates + new Vector2Int(-1, -1);
                    if (!grid.ContainsKey(lowerLeftCoordinates))
                        CreateUpperConnectorWall(
                            coordinates + Vector2Int.down, false);
                }

                return;
            }

            if (lowerTileExists && leftTileExists)
            {
                // Create down left corner wall
                CreateComplexWall(lowerCornerPrefab, coordinates, false);
                var lowerRightCoordinates =
                    coordinates + new Vector2Int(1, -1);
                if (!grid.ContainsKey(lowerRightCoordinates))
                    CreateUpperConnectorWall(coordinates + Vector2Int.down,
                        true);

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
                CreateUpperWall(lowerCoordinates, xMax, yMax);
                if (!grid.ContainsKey(lowerCoordinates + Vector2Int.left))
                    CreateUpperConnectorWall(lowerCoordinates, false);

                if (!grid.ContainsKey(lowerCoordinates + Vector2Int.right))
                    CreateUpperConnectorWall(lowerCoordinates, true);
            }

            if (rightTileExists)
            {
                var rightCoordinates = coordinates + Vector2Int.right;

                // Create right wall
                CreateLeftWall(rightCoordinates);
                if (!grid.ContainsKey(rightCoordinates + Vector2Int.down))
                    CreateLowerConnectorWall(rightCoordinates, false);
            }

            if (leftTileExists)
            {
                var leftCoordinates = coordinates + Vector2Int.left;

                // Create left wall
                CreateRightWall(leftCoordinates);
                if (!grid.ContainsKey(leftCoordinates + Vector2Int.down))
                    CreateLowerConnectorWall(leftCoordinates, true);
            }
        }

        private void CreateUpperWall(Vector2Int coordinates, int xMax, int yMax)
        {
            var topWallPosition = CoordinatesToPosition(coordinates) +
                                  _tileSize * Vector2.up * 0.5f;

            var wallId = GetRandomUpperWallId( coordinates, xMax, yMax);
            var newWall = InstantiateWall(solidWallPrefab[wallId], topWallPosition);
            AdjustWallSpriteLayer(newWall, coordinates, -1);
        }

        private int GetRandomUpperWallId(Vector2Int coordinates, int xMax, int yMax)
        {
            var levelSeed = xMax / 35f + yMax / 55f;
            var x = coordinates.y + coordinates.x / (float)xMax + xMax / 40f;
            var y = (yMax - coordinates.y + coordinates.x) / (float)yMax + yMax / 40f;
            var random1 = Mathf.PerlinNoise(x, y);
            var random2 = Mathf.PerlinNoise(random1, levelSeed);

            int wallId = random2 switch
            {
                < 0.4f => 0,
                < 0.525f => 1,
                < 0.65f => 2,
                < 0.725f => 3,
                _ => 4
            };

            return wallId;
        }

        private void CreateRightWall(Vector2Int coordinates)
        {
            var rightWallPosition = CoordinatesToPosition(coordinates) +
                                    _tileSize * Vector2.right * 0.5f;
            var newWall =
                InstantiateWall(rightWallPrefab, rightWallPosition);
            AdjustWallSpriteLayer(newWall, coordinates);
        }

        private void CreateBottomWall(Vector2Int coordinates)
        {
            var bottomWallPosition = CoordinatesToPosition(coordinates) +
                                     _tileSize * Vector2.down * 0.5f;
            var newWall =
                InstantiateWall(transparentWallPrefab, bottomWallPosition);
            AdjustWallSpriteLayer(newWall, coordinates, 1);
        }

        private void CreateLeftWall(Vector2Int coordinates)
        {
            var leftWallPosition = CoordinatesToPosition(coordinates) +
                                   _tileSize * Vector2.left * 0.5f;
            var newWall = InstantiateWall(leftWallPrefab, leftWallPosition);
            AdjustWallSpriteLayer(newWall, coordinates);
        }

        private void CreateComplexWall(GameObject wallPrefab,
            Vector2Int coordinates, bool facesRight = true)
        {
            var wallPosition = CoordinatesToPosition(coordinates);
            var newWall = InstantiateWall(wallPrefab, wallPosition);
            AdjustWallSpriteLayer(newWall, coordinates);

            if (!facesRight)
                newWall.GetComponentInChildren<SpriteRenderer>().flipX =
                    true;
        }

        private void CreateUpperConnectorWall(Vector2Int coordinates,
            bool facesRight)
        {
            if (facesRight)
            {
                var connectorPosition = CoordinatesToPosition(coordinates) +
                                        new Vector2(_tileSize.x,
                                            _tileSize.y) * 0.5f;
                var newWallConnector =
                    InstantiateWall(solidWallConnectorRightPrefab,
                        connectorPosition);
                AdjustWallSpriteLayer(newWallConnector, coordinates, -1);
            }
            else
            {
                var connectorPosition = CoordinatesToPosition(coordinates) +
                                        new Vector2(-_tileSize.x,
                                            _tileSize.y) * 0.5f;
                var newWallConnector =
                    InstantiateWall(solidWallConnectorLeftPrefab,
                        connectorPosition);
                AdjustWallSpriteLayer(newWallConnector, coordinates, -1);
            }
        }

        private void CreateLowerConnectorWall(Vector2Int coordinates,
            bool facesRight)
        {
            if (facesRight)
            {
                var connectorPosition = CoordinatesToPosition(coordinates) +
                                        new Vector2(_tileSize.x,
                                            -_tileSize.y) * 0.5f;
                var newWallConnector = InstantiateWall(
                    transparentWallConnectorRightPrefab, connectorPosition);

                AdjustWallSpriteLayer(newWallConnector, coordinates, 1);
            }
            else
            {
                var connectorPosition = CoordinatesToPosition(coordinates) +
                                        new Vector2(-_tileSize.x,
                                            -_tileSize.y) * 0.5f;
                var newWallConnector = InstantiateWall(
                    transparentWallConnectorLeftPrefab, connectorPosition);

                AdjustWallSpriteLayer(newWallConnector, coordinates, 1);
            }
        }
    }
}