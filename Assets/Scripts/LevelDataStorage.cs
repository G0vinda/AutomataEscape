using System.Collections.Generic;
using UI;
using UnityEngine;

public static class LevelDataStorage
{
    private static List<LevelData> _levels = new ()
    {
        new LevelData(
            new Vector2Int(0, -1),
            Direction.Up,
            new[,]
            {
                { GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.None, GridManager.TileType.Floor },
                { GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.Goal }
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (StateChartManager.StateAction.MoveForward, 1), 
                new (StateChartManager.StateAction.TurnRight, 1)
            },
            new List<StateChartManager.TransitionCondition>()
            {
                StateChartManager.TransitionCondition.Default
            },
            new Dictionary<Vector2Int, GridManager.KeyType>()),
        new LevelData(
            new Vector2Int(2, -2),
            Direction.Left,
            new[,]
            {
                { GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.Goal},
                { GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.None},
                { GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.Floor}
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (StateChartManager.StateAction.MoveForward, 1), 
                new (StateChartManager.StateAction.TurnRight, 1)
            },
            new List<StateChartManager.TransitionCondition>()
            {
                StateChartManager.TransitionCondition.Default,
                StateChartManager.TransitionCondition.IsInFrontOfWall
            },
            new Dictionary<Vector2Int, GridManager.KeyType>()),
        new LevelData(
            new Vector2Int(0, -4),
            Direction.Up,
            new[,]
            {
                { GridManager.TileType.None, GridManager.TileType.None, GridManager.TileType.None, GridManager.TileType.Goal, GridManager.TileType.Floor, GridManager.TileType.Floor},
                { GridManager.TileType.Orange, GridManager.TileType.Floor, GridManager.TileType.None, GridManager.TileType.None, GridManager.TileType.Floor, GridManager.TileType.Floor},
                { GridManager.TileType.Orange, GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.Floor},
                { GridManager.TileType.Orange, GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.Floor},
                { GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.Floor}
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (StateChartManager.StateAction.MoveForward, 1), 
                new (StateChartManager.StateAction.TurnRight, 1),
                new (StateChartManager.StateAction.TurnLeft, 1)
            },
            new List<StateChartManager.TransitionCondition>()
            {
                StateChartManager.TransitionCondition.Default,
                StateChartManager.TransitionCondition.IsInFrontOfWall,
                StateChartManager.TransitionCondition.StandsOnOrange
            },
            new Dictionary<Vector2Int, GridManager.KeyType>()),
        new LevelData(
            new Vector2Int(6, -4),
            Direction.Up,
            new[,]
            {
                { GridManager.TileType.None, GridManager.TileType.Purple, GridManager.TileType.Orange, GridManager.TileType.None, GridManager.TileType.None, GridManager.TileType.Purple, GridManager.TileType.Orange},
                { GridManager.TileType.Goal, GridManager.TileType.Orange, GridManager.TileType.Purple, GridManager.TileType.Orange, GridManager.TileType.Purple, GridManager.TileType.Orange, GridManager.TileType.Purple},
                { GridManager.TileType.None, GridManager.TileType.Purple, GridManager.TileType.Orange, GridManager.TileType.Purple, GridManager.TileType.Orange, GridManager.TileType.Purple, GridManager.TileType.Orange},
                { GridManager.TileType.None, GridManager.TileType.Orange, GridManager.TileType.Purple, GridManager.TileType.Orange, GridManager.TileType.Purple, GridManager.TileType.Orange, GridManager.TileType.Purple},
                { GridManager.TileType.None, GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.Floor}
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (StateChartManager.StateAction.MoveForward, 2), 
                new (StateChartManager.StateAction.TurnRight, 1),
                new (StateChartManager.StateAction.TurnLeft, 2)
            },
            new List<StateChartManager.TransitionCondition>()
            {
                StateChartManager.TransitionCondition.Default,
                StateChartManager.TransitionCondition.IsInFrontOfWall,
                StateChartManager.TransitionCondition.StandsOnOrange,
                StateChartManager.TransitionCondition.StandsOnPurple
            },
            new Dictionary<Vector2Int, GridManager.KeyType>()),
        new LevelData(
            new Vector2Int(0, 0),
            Direction.Right,
            new[,]
            {
                { GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.Floor, GridManager.TileType.RedGateRight, GridManager.TileType.Goal }
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (StateChartManager.StateAction.MoveForward, 1),
                new (StateChartManager.StateAction.Grab, 1),
                new (StateChartManager.StateAction.Drop, 1)
            },
            new List<StateChartManager.TransitionCondition>()
            {
                StateChartManager.TransitionCondition.Default,
                StateChartManager.TransitionCondition.IsInFrontOfWall,
                StateChartManager.TransitionCondition.StandsOnKey
            },
            new Dictionary<Vector2Int, GridManager.KeyType>()
            {
                {new Vector2Int(2, 0), GridManager.KeyType.Red}
            })
        
    };

    public static LevelData GetLevelData(int id)
    {
        return _levels[id];
    }
}
