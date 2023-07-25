using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using LevelGrid;
using Mono.Collections.Generic;
using Robot.Transitions;
using UI;
using UI.Transition;
using UnityEngine;

namespace Robot.States
{
    public abstract class RobotState
    {
        public enum Status
        {
            Running,
            Pause,
            ReachedGoal
        }
        
        public ReadOnlyCollection<RobotTransition> Transitions => new (_transitions.ToArray());
        public int Id { get; set; }

        protected LevelGridManager LevelGridManager;
        protected SpriteChanger SpriteChanger;

        private List<RobotTransition> _transitions = new();

        protected RobotState(LevelGridManager levelGridManager, SpriteChanger spriteChanger)
        {
            LevelGridManager = levelGridManager;
            SpriteChanger = spriteChanger;
            Id = 0;
        }

        // Returns true if goal is reached
        public abstract Status ProcessState(ref Vector2Int coordinates, ref Direction direction, out Tween animation);

        public int DetermineNextStateId(Vector2Int coordinates, Direction direction)
        {
            foreach (var transition in _transitions)
            {
                if (transition.CheckCondition(coordinates, direction, LevelGridManager))
                    return transition.DestinationId;
            }

            return -1;
        }

        public void AddTransition(RobotTransition newTransition)
        {
            var listIndex = 0;
            foreach (var transition in _transitions)
            {
                if (transition.Priority < newTransition.Priority)
                    break;

                listIndex++;
            }
            
            if(listIndex == _transitions.Count)
            {
                _transitions.Add(newTransition);
            }
            else
            {
                _transitions.Insert(listIndex, newTransition);
            }
        }

        public void RemoveTransition(RobotTransition transition)
        {
            _transitions.Remove(transition);
        }
    }
}