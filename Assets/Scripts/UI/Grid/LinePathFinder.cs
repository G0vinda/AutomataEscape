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
        private Dictionary<SubCell, SubCell> _cameFrom = new ();
        private Dictionary<SubCell, int> _costSoFar = new ();
        
        private static int Heuristic(SubCell a, SubCell b)
        {
            var aCoordinates = a.Coordinates;
            var bCoordinates = b.Coordinates;

            return Math.Abs(aCoordinates.x - bCoordinates.x) + Math.Abs(aCoordinates.y - bCoordinates.y);
        } 
        
        public LinePathFinder(SubCell startPoint, SubCell endPoint, TransitionLine transitionLine, StateUIElement destinationState = null)
        {
            var frontier = new SimplePriorityQueue<SubCell>();
            frontier.Enqueue(startPoint, 0);
            _costSoFar[startPoint] = 0;

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current.Equals(endPoint))
                    break;

                var currentNeighbors = current.GetNeighbors(transitionLine, destinationState);
                foreach (var next in currentNeighbors)
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
            if (startPoint.Equals(endPoint))
                return new List<SubCell> { startPoint };
            
            if (!_cameFrom.ContainsKey(endPoint))
                return null;
            
            var path = new List<SubCell>();
            SubCell current;
            if (destinationState != null)
            {
                var endSubCell = _cameFrom[endPoint];
                while (endSubCell.BlockingState != null)
                {
                    endSubCell = _cameFrom[endSubCell];
                }
                
                path.Add(endSubCell);
                current = endSubCell;
            }
            else
            {
                path.Add(endPoint);
                current = endPoint;
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