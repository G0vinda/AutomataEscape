using System.Collections.Generic;
using Robot;
using UI.State;
using UI.Transition;
using UnityEngine;

namespace UI.Grid
{
    public class SubCell
    {
        public static Dictionary<Vector2Int, SubCell> Grid; 
        private static readonly int MINGridSize = 0;
        private static readonly int MAXGridSize = 21;

        public Vector2Int Coordinates;
            
        public StateUIElement BlockingState;
        public TransitionLine BlockingHorizontalLine;
        public TransitionLine BlockingVerticalLine;

        public static void CreateSubCellGrid()
        {
            Grid = new Dictionary<Vector2Int, SubCell>();
            for (var x = 0; x < MAXGridSize; x++)
            {
                for (var y = 0; y < MAXGridSize; y++)
                {
                    var coordinates = new Vector2Int(x, y);
                    Grid.Add(coordinates, new SubCell(coordinates));
                }
            }
        }

        private SubCell(Vector2Int coordinates)
        {
            Coordinates = coordinates;
        }

        public List<SubCell> GetNeighbors(TransitionLine allowedTransitionLine, StateUIElement allowedStateUIElement)
        {
            var neighbors = new List<SubCell>();
            if (Coordinates.x > MINGridSize) // Add left neighbor
            {
                var leftNeighbor = Grid[Coordinates + Vector2Int.left];
                if (CheckForHorizontalNeighbor(leftNeighbor, allowedStateUIElement, allowedTransitionLine))
                    neighbors.Add(leftNeighbor);
            }

            if (Coordinates.x < MAXGridSize - 1) // Add right neighbor
            {
                var rightNeighbor = Grid[Coordinates + Vector2Int.right];
                if (CheckForHorizontalNeighbor(rightNeighbor, allowedStateUIElement, allowedTransitionLine))
                    neighbors.Add(rightNeighbor);
            }

            if (Coordinates.y > MINGridSize) // Add bottom neighbor 
            {
                var bottomNeighbor = Grid[Coordinates + Vector2Int.down];
                if (CheckForVerticalNeighbor(bottomNeighbor, allowedStateUIElement, allowedTransitionLine))
                    neighbors.Add(bottomNeighbor);
            }

            if (Coordinates.y < MAXGridSize - 1) // Add top neighbor
            {
                var topNeighbor = Grid[Coordinates + Vector2Int.up];
                if (CheckForVerticalNeighbor(topNeighbor, allowedStateUIElement, allowedTransitionLine))
                    neighbors.Add(topNeighbor);
            }

            return neighbors;
        }

        private bool CheckForHorizontalNeighbor(SubCell neighbor, StateUIElement allowedStateUIElement, TransitionLine allowedTransitionLine)
        {
            var originSubCellLineBlock =
                BlockingHorizontalLine == null || BlockingHorizontalLine == allowedTransitionLine;
            var neighborStateBlock = neighbor.BlockingState == null || neighbor.BlockingState == allowedStateUIElement;
            var neighborLineBlock = neighbor.BlockingHorizontalLine == null ||
                                    neighbor.BlockingHorizontalLine == allowedTransitionLine;
            
            return originSubCellLineBlock &&
                   neighborStateBlock &&
                   neighborLineBlock;
        }

        private bool CheckForVerticalNeighbor(SubCell neighbor, StateUIElement allowedStateUIElement, TransitionLine allowedTransitionLine)
        {
            return (BlockingVerticalLine == null || BlockingVerticalLine == allowedTransitionLine) &&
                   (neighbor.BlockingState == null || neighbor.BlockingState == allowedStateUIElement) &&
                   (neighbor.BlockingVerticalLine == null || neighbor.BlockingVerticalLine == allowedTransitionLine);
        }

        public void RemoveBlockingLine(TransitionLine transitionLine)
        {
            if (BlockingVerticalLine == transitionLine)
                BlockingVerticalLine = null;

            if (BlockingHorizontalLine == transitionLine)
                BlockingHorizontalLine = null;
        }
    }
}