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
        new LevelData( // Portal test
            new Vector2Int(0, 0),
            Direction.Down,
            new[,]
            {
                {Floor, None, None },
                {Floor, None, Portal },
                {Portal, None, Floor },
                {None, None, Goal }
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (GoForward, new Vector2Int(3, 3))
            },new List<StateChartManager.TransitionCondition>()
            {
                Default
            },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>(),
            (new Vector2Int(0, -2), new Vector2Int(2, -1))
            ),
        new LevelData( // Level 1 
            new Vector2Int(0, 0),
            Direction.Down,
            new[,]
            {
                {Floor },
                {Floor },
                {Floor },
                {Goal }
            },
            new List<LevelData.AvailableStateInfo>()
            {
                new (GoForward, new Vector2Int(3, 3))
            },new List<StateChartManager.TransitionCondition>()
            {
                Default
            },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()
            ),
        new LevelData(  // Level 2
             new Vector2Int(0, -1),
             Direction.Right,
             new[,]
             {
                 { None, Goal },
                 { Floor, Floor}
             },
             new List<LevelData.AvailableStateInfo>()
             {
                 new (GoForward, new Vector2Int(3, 3)),
                 new (TurnLeft, new Vector2Int(5,3))
             },
             new List<StateChartManager.TransitionCondition>()
             {
                 Default,
             },
             new Dictionary<Vector2Int, LevelGridManager.KeyType>()),


        new LevelData( // Level 3
             new Vector2Int(0, 0),
             Direction.Down,
             new[,]
             {
                 { Floor, None, None},
                 { Floor, Floor, Goal}
             },
             new List<LevelData.AvailableStateInfo>()
             {
                 new (GoForward, new Vector2Int(2, 3)),
                 new (TurnLeft, new Vector2Int(4,3)),
                 new (GoForward, new Vector2Int(6, 3)),
             },
             new List<StateChartManager.TransitionCondition>()
             {
                 Default,
             },
             new Dictionary<Vector2Int, LevelGridManager.KeyType>()),


        new LevelData( // Level 4
             new Vector2Int(0, -2),
             Direction.Left,
             new[,]
             {
                 { Floor, Floor, Goal},
                 { Floor, Floor, Floor},
                 { Floor, Floor, Floor}
             },
             new List<LevelData.AvailableStateInfo>()
             {
                 new (GoForward, new Vector2Int(2, 2)),
                 new (GoForward, new Vector2Int(4, 3)),
                 new (TurnRight, new Vector2Int(2, 5))
             },
             new List<StateChartManager.TransitionCondition>()
             {
                 Default,
                 //IsInFrontOfWall
             },
             new Dictionary<Vector2Int, LevelGridManager.KeyType>()),


        new LevelData( // Level 5
             new Vector2Int(0, -3),
             Direction.Up,
             new[,]
             {
                 { None, None, Goal},
                 { None, Floor, Floor},
                 { Floor, Floor, Floor},
                 { Floor, Floor, None},
             },
             new List<LevelData.AvailableStateInfo>()
             {
                 new (GoForward, new Vector2Int(2, 3)),
                 new (GoForward, new Vector2Int(4, 5)),
                 new (TurnRight, new Vector2Int(3, 1)),
                 new (TurnLeft,  new Vector2Int(5, 3))
             },
             new List<StateChartManager.TransitionCondition>()
             {
                 Default,
             },
             new Dictionary<Vector2Int, LevelGridManager.KeyType>()),


        new LevelData(  //Level 6
             new Vector2Int(0, -2),
             Direction.Up,
             new[,]
             {
                 { Orange, Floor, Orange},
                 { Floor, None, Floor},
                 { Floor, None, Goal}
             },
             new List<LevelData.AvailableStateInfo>()
             {
                 new (GoForward, new Vector2Int(3, 3)),
                 new (TurnRight, new Vector2Int(5, 3)),
             },
             new List<StateChartManager.TransitionCondition>()
             {
                 Default,
                 StandsOnOrange
             },
             new Dictionary<Vector2Int, LevelGridManager.KeyType>()),
        // new Dictionary<Vector2Int, LevelGridManager.KeyType>()
        //  {
        //     {new Vector2Int(0, -1), LevelGridManager.KeyType.Red}  // <-- Wenn Gears 
        //  }),

        //
        new LevelData( // Level 7
             new Vector2Int(0, 0),
             Direction.Down,
             new[,]
             {
                 { Floor, None, None, None, None},
                 { Floor, None, None, None, None},
                 { Orange, Floor, Orange, Floor, None},
                 { None, Floor, Floor, Floor, None },
                 { None, Floor, Orange, Floor, Goal }
             },
             new List<LevelData.AvailableStateInfo>()
             {
                 new (GoForward, new Vector2Int(1, 5)),
                 new (GoForward, new Vector2Int(6, 4)),
                 new (TurnLeft,  new Vector2Int(4, 3)),
                 new (TurnRight, new Vector2Int(3, 1))
             },
             new List<StateChartManager.TransitionCondition>()
             {
                 Default,
                 StandsOnOrange,
             },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()),


        new LevelData( // Level 8
             new Vector2Int(0, -2),
             Direction.Right,
             new[,]
             {
                 { None, None, Goal},
                 { None, None, Floor},
                 { Floor, Floor, Floor},
             },
             new List<LevelData.AvailableStateInfo>()
             {
                 new (GoForward, new Vector2Int(2, 3)),
                 new (TurnLeft, new Vector2Int(5,3))
             },
             new List<StateChartManager.TransitionCondition>()
             {
                 Default,
                 IsInFrontOfWall
             },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()),


        new LevelData( // Level 9
             new Vector2Int(0, -1),
             Direction.Right,
             new[,]
             {
                 {Floor, None, Floor, Floor, Floor},
                 {Floor, Floor, Floor, None, Goal},
                 {Floor, None, Floor, Floor, Floor},
             },
             new List<LevelData.AvailableStateInfo>()
             {
                 new (GoForward, new Vector2Int(2, 5)),
                 new (GoForward, new Vector2Int(5, 2)),
                 new (TurnLeft, new Vector2Int(5,5)),
                 new (TurnRight, new Vector2Int(2,2))
             },
             new List<StateChartManager.TransitionCondition>()
             {
                 Default,
                 IsInFrontOfWall
             },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()),


        new LevelData( // Level 10
             new Vector2Int(0, -4),
             Direction.Up,
             new[,]
             {
                 {None, None, Goal},
                 {Floor, None, Floor},
                 {Orange, Floor, Floor},
                 {Floor, None, None },
                 {Floor, None, None },
             },
             new List<LevelData.AvailableStateInfo>()
             {
                 new (GoForward, new Vector2Int(2, 3)),
                 new (TurnRight, new Vector2Int(3, 1)),
                 new (TurnLeft,  new Vector2Int(4 ,3))
             },
             new List<StateChartManager.TransitionCondition>()
             {
                 Default,
                 IsInFrontOfWall,
                 StandsOnOrange,

             },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()),


        new LevelData( // Level 11
             new Vector2Int(0, -3),
             Direction.Right,
             new[,]
             {
                 {None, None, Goal, None, None},
                 {None, None, Floor, None, None},
                 {None, Floor, Orange, Floor, None},
                 {Floor, Floor, Floor, Orange, Floor},
             },
             new List<LevelData.AvailableStateInfo>()
             {
                 new (GoForward, new Vector2Int(2, 4)),
                 new (GoForward, new Vector2Int(2, 1)),
                 new (TurnRight, new Vector2Int(5, 1)),
                 new (TurnLeft,  new Vector2Int(5 ,4))
             },
             new List<StateChartManager.TransitionCondition>()
             {
                 Default,
                 IsInFrontOfWall,
                 StandsOnOrange,

             },
            new Dictionary<Vector2Int, LevelGridManager.KeyType>()),


        new LevelData( // Level 12
             new Vector2Int(0, 0),
             Direction.Right,
             new[,]
             {
                 {Floor, Floor, Floor, RedGateRight, Goal }
             },
             new List<LevelData.AvailableStateInfo>()
             {
                 new (GoForward, new Vector2Int(2, 3)),
                 new (Drop, new Vector2Int(2, 5)),
                 new (Grab, new Vector2Int(5, 3)),
             },
             new List<StateChartManager.TransitionCondition>()
             {
                 Default,
                 IsInFrontOfWall,
                 StandsOnKey,
             },
             new Dictionary<Vector2Int, LevelGridManager.KeyType>()
             {
                {new Vector2Int(1, 0), LevelGridManager.KeyType.Red}
             }),


        new LevelData( // Level 13
             new Vector2Int(0, -3),
             Direction.Up,
             new[,]
             {
                 { Floor, None, None },
                 { Floor, RedGateRight, Goal },
                 { Floor, None, None },
                 { Floor, None, None },
             },
             new List<LevelData.AvailableStateInfo>()
             {
                 new (GoForward, new Vector2Int(2, 2)),
                 new (TurnRight, new Vector2Int(4, 5)),
                 new (Drop, new Vector2Int(5, 2)),
                 new (Grab, new Vector2Int(2, 5)),
             },
             new List<StateChartManager.TransitionCondition>()
             {
                 Default,
                 IsInFrontOfWall,
                 StandsOnKey,
             },
             new Dictionary<Vector2Int, LevelGridManager.KeyType>()
             {
                {new Vector2Int(0, -1), LevelGridManager.KeyType.Red}
             }),


        new LevelData( // Level 14
             new Vector2Int(0, 0),
             Direction.Down,
             new[,]
             {
                 { Floor, None, None, None },
                 { Floor, None, None, None },
                 { Floor, Floor, RedGateRight, Goal },
             },
             new List<LevelData.AvailableStateInfo>()
             {
                 new (GoForward, new Vector2Int(2, 4)),
                 new (TurnLeft, new Vector2Int(5, 4)),
                 new (Drop, new Vector2Int(5, 2)),
                 new (Grab, new Vector2Int(2, 2)),
             },
             new List<StateChartManager.TransitionCondition>()
             {
                 Default,
                 IsInFrontOfWall,
                 StandsOnKey,
             },
             new Dictionary<Vector2Int, LevelGridManager.KeyType>()
             {
                {new Vector2Int(0, -1), LevelGridManager.KeyType.Red}
             }),


        new LevelData( // Level 15
             new Vector2Int(1, -4),
             Direction.Down,
             new[,]
             {
                { None, None, None, None, None, Floor, None, None },
                { None, None, None, None, None, Floor, None, None },
                { Floor, Floor, Floor, RedGateRight, Floor, Floor, BlueGateRight, Goal },
                { Floor, Floor, None, None, None, Floor, None, None },
                { None, Floor, None, None, None, Floor, None, None }
             },
             new List<LevelData.AvailableStateInfo>()
             {
                 new (GoForward, new Vector2Int(2, 4)),
                 new (TurnRight, new Vector2Int(4, 4)),
                 new (Drop, new Vector2Int(6, 4)),
                 new (Grab, new Vector2Int(2, 1)),
             },
             new List<StateChartManager.TransitionCondition>()
             {
                 Default,
                 IsInFrontOfWall,
                 StandsOnKey,
             },
             new Dictionary<Vector2Int, LevelGridManager.KeyType>()
             {
                {new Vector2Int(1, -2), LevelGridManager.KeyType.Red},
                {new Vector2Int(0, -2), LevelGridManager.KeyType.Blue}
             }),

        // new LevelData( // Level 16
        //     new Vector2Int(0, -2),
        //     Direction.Right,
        //     new[,]
        //     {
        //         { None, None, Floor, Floor, None, Goal },
        //         { None, Floor, Floor, Floor, None, Floor },
        //         { Floor, Floor, None, Floor, Floor, Floor },
        //         { None, Floor, None, None, Floor, Floor }
        //      
        //     },
        //     new List<LevelData.AvailableStateInfo>()
        //     {
        //         new (GoForward, 3),
        //         new (TurnRight, 1),
        //         new (TurnLeft, 1),
        //     },
        //     new List<StateChartManager.TransitionCondition>()
        //     {
        //         Default,
        //         IsInFrontOfWall,
        //     },
        //    new Dictionary<Vector2Int, LevelGridManager.KeyType>())
    };

    public static LevelData GetLevelData(int id)
    {
        return _levels[id];
    }
}
