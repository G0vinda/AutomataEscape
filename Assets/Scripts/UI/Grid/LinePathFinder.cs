using System;
using System.Collections.Generic;
using System.Linq;
using Priority_Queue;
using UI.State;
using UI.Transition;
using UnityEngine;

namespace UI.Grid
{
    public class LinePathFinder
    {
        private SimplePriorityQueue<SubCell> _frontier = new ();
        private Dictionary<SubCell, SubCell> _cameFrom = new ();
        private Dictionary<SubCell, int> _costSoFar = new ();
        private SubCell _startPoint;
        private SubCell _endPoint;

        public LinePathFinder(SubCell startPoint, SubCell endPoint)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;
        }
        
        public List<SubCell> GetStartPath(StateUIElement startState)
        {
            _frontier = new SimplePriorityQueue<SubCell>();
            _costSoFar = new Dictionary<SubCell, int>();
            _frontier.Enqueue(_startPoint, 0);
            _costSoFar[_startPoint] = 0;
            
            while (_frontier.Count > 0)
            {
                var current = _frontier.Dequeue();

                if (current.Equals(_endPoint))
                    break;

                var currentNeighbors = current.GetNeighbors(null, startState);
                foreach (var next in currentNeighbors)
                {
                    var newCost = _costSoFar[current] + 1;
                    if (!_costSoFar.ContainsKey(next) || newCost < _costSoFar[next])
                    {
                        _costSoFar[next] = newCost;
                        var priority = newCost + Heuristic(next, _endPoint);
                        _frontier.Enqueue(next, priority);
                        _cameFrom[next] = current;
                    }
                }
            }
            return ExtractPath(null, startState);
        }

        public List<SubCell> GetPath(TransitionLine transitionLine, StateUIElement destinationState = null)
        {
            _frontier = new SimplePriorityQueue<SubCell>();
            _frontier.Enqueue(_startPoint, 0);
            _costSoFar = new();
            _costSoFar[_startPoint] = 0;
            
            while (_frontier.Count > 0)
            {
                var current = _frontier.Dequeue();

                if (current.Equals(_endPoint))
                    break;

                var currentNeighbors = current.GetNeighbors(transitionLine, destinationState);
                foreach (var next in currentNeighbors)
                {
                    var newCost = _costSoFar[current] + 1;
                    if (!_costSoFar.ContainsKey(next) || newCost < _costSoFar[next])
                    {
                        _costSoFar[next] = newCost;
                        var priority = newCost + Heuristic(next, _endPoint);
                        _frontier.Enqueue(next, priority);
                        _cameFrom[next] = current;
                    }
                }
            }

            return ExtractPath(destinationState, null);
        }
        
        private static int Heuristic(SubCell a, SubCell b)
        {
            var aCoordinates = a.Coordinates;
            var bCoordinates = b.Coordinates;

            return Math.Abs(aCoordinates.x - bCoordinates.x) + Math.Abs(aCoordinates.y - bCoordinates.y);
        } 
        
        private List<SubCell> ExtractPath(StateUIElement destinationState, StateUIElement startState)
        {
            if (_startPoint.Equals(_endPoint))
                return new List<SubCell> { _startPoint };
            
            if (!_cameFrom.ContainsKey(_endPoint))
                return null;
            
            var path = new List<SubCell>();
            SubCell current;
            if (destinationState != null)
            {
                var endSubCell = _cameFrom[_endPoint];
                while (endSubCell.BlockingState != null)
                {
                    endSubCell = _cameFrom[endSubCell];
                }
                
                path.Add(endSubCell);
                current = endSubCell;
            }
            else
            {
                path.Add(_endPoint);
                current = _endPoint;
            }
            
            while (!current.Equals(_startPoint))
            {
                var prev = _cameFrom[current];
                if (startState != null && prev.BlockingState == startState)
                    break;
                path.Insert(0, prev);
                current = prev;
            }

            return path;
        }   
    }
}