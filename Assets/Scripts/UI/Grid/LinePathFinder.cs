using System;
using System.Collections.Generic;
using Priority_Queue;
using UI.State;
using UI.Transition;
using UnityEngine;

namespace UI.Grid
{
    public class LinePathFinder
    {
        public List<SubCell> Path;
        private Dictionary<SubCell, SubCell> _cameFrom = new Dictionary<SubCell, SubCell>();
        private Dictionary<SubCell, int> _costSoFar = new Dictionary<SubCell, int>();
        
        private static int Heuristic(SubCell a, SubCell b)
        {
            var aCoordinates = a.Coordinates;
            var bCoordinates = b.Coordinates;

            return Math.Abs(aCoordinates.x - bCoordinates.x) + Math.Abs(aCoordinates.y - bCoordinates.y);
        } 
        
        public LinePathFinder(Dictionary<Vector2Int, SubCell> grid, SubCell startPoint, SubCell endPoint, TransitionLine transitionLine, StateUIElement destinationState = null)
        {
            var frontier = new SimplePriorityQueue<SubCell>();
            frontier.Enqueue(startPoint, 0);

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();
                _costSoFar[current] = 0;

                if (current.Equals(endPoint))
                {
                    break;
                }

                foreach (var next in current.GetNeighbors(transitionLine, destinationState))
                {
                    var newCost = _costSoFar[current] + 1;
                    if (!_costSoFar.ContainsKey(next) || newCost < _costSoFar[next])
                    {
                        _costSoFar[next] = newCost;
                        var priority = newCost + Heuristic(next, endPoint);
                        frontier.Enqueue(next, priority);
                        _cameFrom[next] = current;
                    }
                }
            }

            Path = SetPath(startPoint, endPoint, destinationState);
        }

        private List<SubCell> SetPath(SubCell startPoint, SubCell endPoint, StateUIElement destinationState)
        {
            if (!_cameFrom.ContainsKey(endPoint))
                return null;
            
            var path = new List<SubCell>();
            var current = endPoint;
            if (destinationState != null)
            {
                var endSubCell = _cameFrom[endPoint];
                while (endSubCell.BlockingState != null)
                {
                    endSubCell = _cameFrom[endSubCell];
                }
                
                path.Add(endSubCell);
            }
            else
            {
                path.Add(endPoint);    
            }
            
            while (!current.Equals(startPoint))
            {
                var prev = _cameFrom[current];
                path.Insert(0, prev);
                current = prev;
            }

            return path;
        }   
    }
}