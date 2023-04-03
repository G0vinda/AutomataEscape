using System.Collections.Generic;
using Robot;
using UI;
using UI.Transition;
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
                { LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.None, LevelGridManager.TileType.Floor },
                { LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Goal }
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (StateChartManager.StateAction.GoForward, 1), 
                new (StateChartManager.StateAction.TurnRight, 1)
            },
            new List<StateChartManager.TransitionCondition>()
            {
                StateChartManager.TransitionCondition.Default
            },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()),
        new LevelData(
            new Vector2Int(2, -2),
            Direction.Left,
            new[,]
            {
                { LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Goal},
                { LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.None},
                { LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor}
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (StateChartManager.StateAction.GoForward, 1), 
                new (StateChartManager.StateAction.TurnRight, 1)
            },
            new List<StateChartManager.TransitionCondition>()
            {
                StateChartManager.TransitionCondition.Default,
                StateChartManager.TransitionCondition.IsInFrontOfWall
            },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()),
        new LevelData(
            new Vector2Int(0, -4),
            Direction.Up,
            new[,]
            {
                { LevelGridManager.TileType.None, LevelGridManager.TileType.None, LevelGridManager.TileType.None, LevelGridManager.TileType.Goal, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor},
                { LevelGridManager.TileType.Orange, LevelGridManager.TileType.Floor, LevelGridManager.TileType.None, LevelGridManager.TileType.None, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor},
                { LevelGridManager.TileType.Orange, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor},
                { LevelGridManager.TileType.Orange, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor},
                { LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor}
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (StateChartManager.StateAction.GoForward, 1), 
                new (StateChartManager.StateAction.TurnRight, 1),
                new (StateChartManager.StateAction.TurnLeft, 1)
            },
            new List<StateChartManager.TransitionCondition>()
            {
                StateChartManager.TransitionCondition.Default,
                StateChartManager.TransitionCondition.IsInFrontOfWall,
                StateChartManager.TransitionCondition.StandsOnOrange
            },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()),
        new LevelData(
            new Vector2Int(6, -4),
            Direction.Up,
            new[,]
            {
                { LevelGridManager.TileType.None, LevelGridManager.TileType.Purple, LevelGridManager.TileType.Orange, LevelGridManager.TileType.None, LevelGridManager.TileType.None, LevelGridManager.TileType.Purple, LevelGridManager.TileType.Orange},
                { LevelGridManager.TileType.Goal, LevelGridManager.TileType.Orange, LevelGridManager.TileType.Purple, LevelGridManager.TileType.Orange, LevelGridManager.TileType.Purple, LevelGridManager.TileType.Orange, LevelGridManager.TileType.Purple},
                { LevelGridManager.TileType.None, LevelGridManager.TileType.Purple, LevelGridManager.TileType.Orange, LevelGridManager.TileType.Purple, LevelGridManager.TileType.Orange, LevelGridManager.TileType.Purple, LevelGridManager.TileType.Orange},
                { LevelGridManager.TileType.None, LevelGridManager.TileType.Orange, LevelGridManager.TileType.Purple, LevelGridManager.TileType.Orange, LevelGridManager.TileType.Purple, LevelGridManager.TileType.Orange, LevelGridManager.TileType.Purple},
                { LevelGridManager.TileType.None, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor}
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (StateChartManager.StateAction.GoForward, 2), 
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
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()),
        new LevelData(
            new Vector2Int(0, 0),
            Direction.Right,
            new[,]
            {
                { LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.RedGateRight, LevelGridManager.TileType.Goal }
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (StateChartManager.StateAction.GoForward, 1),
                new (StateChartManager.StateAction.Grab, 1),
                new (StateChartManager.StateAction.Drop, 1)
            },
            new List<StateChartManager.TransitionCondition>()
            {
                StateChartManager.TransitionCondition.Default,
                StateChartManager.TransitionCondition.IsInFrontOfWall,
                StateChartManager.TransitionCondition.StandsOnKey
            },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()
            {
                {new Vector2Int(2, 0), LevelGridManager.KeyType.Red}
            }),
        new LevelData(
            new Vector2Int(0, 0),
            Direction.Right,
            new[,]
            {
                { LevelGridManager.TileType.Floor, LevelGridManager.TileType.None, LevelGridManager.TileType.None, LevelGridManager.TileType.None, LevelGridManager.TileType.None },
                { LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.Floor, LevelGridManager.TileType.RedGateRight, LevelGridManager.TileType.Goal }
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (StateChartManager.StateAction.GoForward, 4),
                new (StateChartManager.StateAction.TurnLeft, 4),
                new (StateChartManager.StateAction.TurnRight, 4),
                new (StateChartManager.StateAction.Grab, 4),
                new (StateChartManager.StateAction.Drop, 4)
            },
            new List<StateChartManager.TransitionCondition>()
            {
                StateChartManager.TransitionCondition.Default,
                StateChartManager.TransitionCondition.IsInFrontOfWall,
                StateChartManager.TransitionCondition.StandsOnKey,
                StateChartManager.TransitionCondition.StandsOnOrange,
                StateChartManager.TransitionCondition.StandsOnPurple,
                
            },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()
            {
                {new Vector2Int(2, -1), LevelGridManager.KeyType.Red}
            })
        
    };

    public static LevelData GetLevelData(int id)
    {
        return _levels[id];
    }
}
