using System.Collections.Generic;
using Tiles;
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
    
    public (int, int) RobotStartPosition { get; }
    public Quaternion RobotStartRotation { get; }
    public Tile.TileType[,] Grid { get; }

    public List<AvailableStateInfo> AvailableActions;
    public List<StateChartManager.TransitionCondition> AvailableTransitionConditions;

    public LevelData((int, int) startPos, Quaternion startRot, Tile.TileType[,] grid,
        List<AvailableStateInfo> actions, List<StateChartManager.TransitionCondition> conditions)
    {
        RobotStartPosition = startPos;
        RobotStartRotation = startRot;
        Grid = grid;
        AvailableActions = actions;
        AvailableTransitionConditions = conditions;
    }
}

public class LevelWithKeyData : LevelData
{
    public (int, int) KeyPosition { get; }

    public LevelWithKeyData((int, int) startPos, (int, int) keyPos, Quaternion startRot,
        Tile.TileType[,] grid, List<AvailableStateInfo> actions,
        List<StateChartManager.TransitionCondition> conditions) : base(startPos, startRot, grid, actions, conditions)
    {
        KeyPosition = keyPos;
    }
}