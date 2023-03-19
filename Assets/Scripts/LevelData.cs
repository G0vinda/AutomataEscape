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
    public Tile.TileType[,] Grid { get; }

    public List<AvailableStateInfo> AvailableActions;
    public List<StateChartManager.TransitionCondition> AvailableTransitionConditions;

    public LevelData(Vector2Int startPos, Direction startDirection, Tile.TileType[,] grid,
        List<AvailableStateInfo> actions, List<StateChartManager.TransitionCondition> conditions)
    {
        RobotStartPosition = startPos;
        RobotStartDirection = startDirection;
        Grid = grid;
        AvailableActions = actions;
        AvailableTransitionConditions = conditions;
    }
}

public class LevelWithKeyData : LevelData
{
    public Vector2Int KeyPosition { get; }

    public LevelWithKeyData(Vector2Int startPos, Vector2Int keyPos, Direction startDirection,
        Tile.TileType[,] grid, List<AvailableStateInfo> actions,
        List<StateChartManager.TransitionCondition> conditions) : base(startPos, startDirection, grid, actions, conditions)
    {
        KeyPosition = keyPos;
    }
}