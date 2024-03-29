﻿using DG.Tweening;
using Helper;
using LevelGrid;
using UI.Transition;
using UnityEngine;

namespace Robot.States
{
    public class GoForwardState : RobotState
    {
        private Transform _robotTransform;
        public GoForwardState(LevelGridManager levelGridManager, SpriteChanger spriteChanger, Transform robotTransform) : base(
            levelGridManager, spriteChanger)
        {
            _robotTransform = robotTransform;
        }

        public override Status ProcessState(ref Vector2Int coordinates, ref Direction direction, out Tween animation)
        {
            animation = null;
            
            if (LevelGridManager.CheckIfWayIsBlocked(coordinates, direction)) // Robot will bump into wall
            {
                var bumpIntoWallTime = 0.2f;
                var bumpRetreatTime = 0.4f;
                var bumpDistance = 0.9f * (Vector2)direction.ToVector2Int();

                if (KeyHandleState.GetCurrentKeyType() != LevelGridManager.KeyType.None)
                {
                    bumpIntoWallTime *= 0.3f;
                    bumpRetreatTime *= 0.3f;
                    bumpDistance *= 0.3f;
                }
                
                var robotStartPosition = _robotTransform.position;
                var bumpPosition = robotStartPosition + (Vector3)bumpDistance;

                var animationSequence = DOTween.Sequence();
                SpriteChanger.GoForward();
                SoundPlayer.Instance.PlayRobotHitWall();
                animationSequence.Append(_robotTransform.DOMove(bumpPosition, bumpIntoWallTime)
                    .SetEase(Ease.InCubic));
                animationSequence.Append(_robotTransform.DOMove(robotStartPosition, bumpRetreatTime)
                    .SetEase(Ease.OutSine));
                animation = animationSequence;
                   
                return Status.Running;    
            }
            
            var moveTime = 0.6f;
            coordinates += direction.ToVector2Int();
            animation = _robotTransform.DOMove(LevelGridManager.Grid[coordinates].transform.position, moveTime).SetEase(Ease.InOutSine);
            SpriteChanger.GoForward();
            SpriteChanger.SetSpriteSortingOrder(LevelGridManager.GetSpriteSortingOrderFromCoordinates(coordinates));
            SoundPlayer.Instance.PlayRobotMove();
            GameManager.Instance.CheckForEnemyCollision();

            if (LevelGridManager.CheckIfTileIsPortal(coordinates, out var portalAnimator))
            {
                var startPortalCoordinates = coordinates;
                animation.onComplete = () =>
                {
                    GameManager.Instance.InitiateMoveThroughPortal(startPortalCoordinates);
                    portalAnimator.PlayTeleport();
                };
                
                return Status.Pause;
            }

            if (LevelGridManager.CheckIfTileIsGoal(coordinates, out var goalAnimator))
            {
                goalAnimator.PlayTeleport();
                return Status.ReachedGoal;
            }
            
            return Status.Running;
        }
    }
}