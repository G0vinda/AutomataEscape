using System.Collections.Generic;
using LevelGrid;
using Robot;
using static LevelGrid.LevelGridManager.TileType;
using static Robot.StateChartManager.StateAction;
using static Robot.StateChartManager.TransitionCondition;
using UI.Transition;
using UnityEngine;

public static class LevelDataStorage
{
    public static int LevelCount => _levels.Count;
    
    private static List<LevelData> _levels = new ()
    {
        new LevelData( // Level 1
            new Vector2Int(0, 0),
            Direction.Right,
            new[,]
            {
                { Floor, Floor, Floor, Floor, Goal }
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (GoForward, 1), 
            },
            new List<StateChartManager.TransitionCondition>()
            {
                Default
            },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()), // <-- Wenn keine Gears 
        new LevelData( 
            new Vector2Int(2, 0),
            Direction.Down,
            new[,]
            {
                { Floor, Floor, Floor},
                { Goal, Floor, Floor},
                { None, Floor, Floor}
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (GoForward, 2), 
                new (TurnRight, 1)
            },
            new List<StateChartManager.TransitionCondition>()
            {
                Default,
            },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()),
        new LevelData(
            new Vector2Int(3, 0),
            Direction.Left,
            new[,]
            {
                { Floor, Floor, Floor, Floor},
                { Floor, Floor, None, None},
                { Floor, Floor, Floor, Goal},
                { Floor, Floor, Floor, None}
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (GoForward, 2), 
                new (TurnLeft, 1)
            },
            new List<StateChartManager.TransitionCondition>()
            {
                Default,
            },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()),
        new LevelData(
            new Vector2Int(2, -2),
            Direction.Left,
            new[,]
            {
                { Floor, Floor, Goal},
                { Floor, Floor, None},
                { Floor, Floor, Floor}
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (GoForward, 1), 
                new (TurnRight, 1)
            },
            new List<StateChartManager.TransitionCondition>()
            {
                Default,
                IsInFrontOfWall
            },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()),
        new LevelData(
            new Vector2Int(2, -5),
            Direction.Up,
            new[,]
            {
                { Goal, None, Floor, Floor},
                { Floor, Floor, Floor, Floor},
                { Floor, None, Floor, Floor},
                { Floor, Floor, Floor, Floor},
                { None, Floor, Floor, Floor},
                { None, Floor, Floor, Floor}
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (GoForward, 2), 
                new (TurnRight, 1),
                new (TurnLeft, 1),
            },
            new List<StateChartManager.TransitionCondition>()
            {
                Default,
                IsInFrontOfWall
            },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()),
        
        new LevelData(
            new Vector2Int(0, -3),
            Direction.Up,
            new[,]
            {
                { Floor, None, Floor},
                { Floor, RedGateRight, Goal},
                { Floor, None, Floor},
                { Floor, None, Floor}
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (GoForward, 1), 
                new (TurnRight, 1),
                new (Grab, 1),
                new (Drop, 1)
            },
            new List<StateChartManager.TransitionCondition>()
            {
                Default,
                IsInFrontOfWall,
                StandsOnKey
            },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()
            {
                {new Vector2Int(0, -1), LevelGridManager.KeyType.Red}  // <-- Wenn Gears 
            }),
        new LevelData(
            new Vector2Int(4, -4),
            Direction.Down,
            new[,]
            {
                { None, None, Floor, None, None, None},
                { None, None, Floor, None, None, None},
                { Goal, BlueGateLeft, Floor, RedGateLeft, Floor, Floor},
                { None, None, Floor, None, Floor, Floor},
                { None, None, Floor, None, Floor, None}
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (GoForward, 1), 
                new (TurnLeft, 1),
                new (Grab, 1),
                new (Drop, 1)
            },
            new List<StateChartManager.TransitionCondition>()
            {
                Default,
                IsInFrontOfWall,
                StandsOnKey
            },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()
            {
                {new Vector2Int(4, -2), LevelGridManager.KeyType.Red},
                {new Vector2Int(5, -2), LevelGridManager.KeyType.Blue},
            }),
        new LevelData(
            new Vector2Int(0, -4),
            Direction.Up,
            new[,]
            {
                { None, None, None, Goal, Floor},
                { Orange, Floor, None, None, Floor},
                { Orange, Floor, Floor, Floor, Floor},
                { Orange, Floor, Floor, Floor, Floor},
                { Floor, Floor, Floor, Floor, None}
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (GoForward, 1), 
                new (TurnRight, 1),
                new (TurnLeft, 1)
            },
            new List<StateChartManager.TransitionCondition>()
            {
                Default,
                IsInFrontOfWall,
                StandsOnOrange
            },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()),
        new LevelData(
            new Vector2Int(2, -4),
            Direction.Right,
            new[,]
            {
                { Floor, None, Orange, None, None, Purple, Orange},
                { Goal, RedGateLeft, Purple, Orange, Purple, Orange, Purple},
                { None, None, Orange, Purple, Orange, Purple, Orange},
                { None, None, Purple, Orange, Purple, Orange, Purple},
                { None, None, Floor, Floor, Floor, Floor, Floor}
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (GoForward, 2), 
                new (TurnRight, 1),
                new (TurnLeft, 2),
                new (Grab, 1),
                new (Drop, 1)
            },
            new List<StateChartManager.TransitionCondition>()
            {
                Default,
                IsInFrontOfWall,
                StandsOnKey,
                StandsOnOrange,
                StandsOnPurple
            },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()
            {
                {new Vector2Int(6, -4), LevelGridManager.KeyType.Red},    
            }),
    };

    public static LevelData GetLevelData(int id)
    {
        return _levels[id];
    }
}
