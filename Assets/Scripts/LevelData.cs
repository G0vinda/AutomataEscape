using System.Collections.Generic;
using LevelGrid;
using Robot;
using UI.Transition;
using UnityEngine;

public class LevelData
{
    public readonly struct AvailableStateInfo
    {
        public StateChartManager.StateAction Action { get; }
        public Vector2Int StartPositionOnGrid { get; }

        public AvailableStateInfo(StateChartManager.StateAction action, Vector2Int startPosition)
        {
            Action = action;
            StartPositionOnGrid = startPosition;
        }
    }
    
    public Vector2Int RobotStartPosition { get; }
    public Direction RobotStartDirection { get; }
    public LevelGridManager.TileType[,] Grid { get; }

    public readonly List<AvailableStateInfo> AvailableActions;
    public readonly List<StateChartManager.TransitionCondition> AvailableTransitionConditions;
    public readonly Dictionary<Vector2Int, LevelGridManager.KeyType> KeyData;

    public LevelData(
        Vector2Int startPos, 
        Direction startDirection, 
        LevelGridManager.TileType[,] grid,
        List<AvailableStateInfo> actions, 
        List<StateChartManager.TransitionCondition> conditions,
        Dictionary<Vector2Int, LevelGridManager.KeyType> keyData)
    {
        RobotStartPosition = startPos;
        RobotStartDirection = startDirection;
        Grid = grid;
        AvailableActions = actions;
        AvailableTransitionConditions = conditions;
        KeyData = keyData;
    }
}