using System.Collections.Generic;
using Tiles;
using UI;
using UnityEngine;

public class LevelData
{
    public class AvailableStateInfo
    {
        public StateChartManager.StateAction Action { get; }
        public int Amount { get; }

        public AvailableStateInfo(StateChartManager.StateAction action, int amount)
        {
            Action = action;
            Amount = amount;
        }
    }
    
    public Vector2Int RobotStartPosition { get; }
    public Direction RobotStartDirection { get; }
    public GridManager.TileType[,] Grid { get; }

    public List<AvailableStateInfo> AvailableActions;
    public List<StateChartManager.TransitionCondition> AvailableTransitionConditions;
    public Dictionary<Vector2Int, GridManager.KeyType> KeyData;

    public LevelData(
        Vector2Int startPos, 
        Direction startDirection, 
        GridManager.TileType[,] grid,
        List<AvailableStateInfo> actions, 
        List<StateChartManager.TransitionCondition> conditions,
        Dictionary<Vector2Int, GridManager.KeyType> keyData)
    {
        RobotStartPosition = startPos;
        RobotStartDirection = startDirection;
        Grid = grid;
        AvailableActions = actions;
        AvailableTransitionConditions = conditions;
        KeyData = keyData;
    }
}