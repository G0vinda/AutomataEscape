using System;
using System.Collections.Generic;
using DG.Tweening;
using LevelGrid;
using UI;
using UI.Transition;
using UnityEngine;

namespace Robot
{
    public class SpriteChanger : MonoBehaviour
    {
        [Header("Animators")] 
        [SerializeField] private Animator headGateAnimator;
        [SerializeField] private Animator headAnimator;
        [SerializeField] private Animator bodyAnimator;

        [Header("SpriteRenderers")] 
        [SerializeField] private SpriteRenderer headGateSpriteRenderer;
        [SerializeField] private SpriteRenderer headSpriteRenderer;
        [SerializeField] private SpriteRenderer bodySpriteRenderer;

        [Header("RobotHeadSprites")] 
        [SerializeField] private Sprite offHeadSprite; 
            
        [Header("RobotBodySprites")]
        [SerializeField] private Sprite upRobot;
        [SerializeField] private Sprite sideRobot;
        [SerializeField] private Sprite downRobot;

        [SerializeField] private Sprite upRobotWithBlueKey;
        [SerializeField] private Sprite sideRobotWithBlueKey;
        [SerializeField] private Sprite downRobotWithBlueKey;

        [SerializeField] private Sprite upRobotWithRedKey;
        [SerializeField] private Sprite sideRobotWithRedKey;
        [SerializeField] private Sprite downRobotWithRedKey;

        [Header("BeamParticles")] 
        [SerializeField] private ParticleSystem frontParticles;
        [SerializeField] private ParticleSystem backParticles;

        [Header("BeamValues")] 
        [SerializeField] private Color beamTransparentColor;
        [SerializeField] private Color beamSolidColor;

        private Sprite upSprite;
        private Sprite sideSprite;
        private Sprite downSprite;
        
        private LevelGridManager.KeyType _keyState;
        private Direction _direction;

        private int _robotStartUpHash;
        private int _robotShutDownHash;
        private int _robotOnIdleHash;
        private int _robotOffHash;
        
        private int _robotHeadOpenFrontHash;
        private int _robotHeadOpenSideHash;
        private int _robotHeadCloseFrontHash;
        private int _robotHeadCloseSideHash;
        private int _robotHeadIsClosedFront;
        private int _robotHeadIsClosedSide;

        private Dictionary<(Direction, LevelGridManager.KeyType), int> _idleAnimations;
        private Dictionary<Direction, int> _idleClosedAnimations;
        private Dictionary<(Direction, LevelGridManager.KeyType), int> _moveAnimations;
        private Dictionary<(Direction, LevelGridManager.KeyType), int> _reverseMoveAnimations;
        private Dictionary<(Direction, Direction, LevelGridManager.KeyType), int> _turnAnimations;

        private void Awake()
        {
            // Head animations

            _robotStartUpHash = Animator.StringToHash("RobotStartUp");
            _robotShutDownHash = Animator.StringToHash("RobotShutDown");
            _robotOnIdleHash = Animator.StringToHash("RobotOnIdle");
            _robotOffHash = Animator.StringToHash("RobotOff");

            _robotHeadOpenFrontHash = Animator.StringToHash("RobotHeadOpenFront");
            _robotHeadOpenSideHash = Animator.StringToHash("RobotHeadOpenSide");
            _robotHeadCloseFrontHash = Animator.StringToHash("RobotHeadCloseFront");
            _robotHeadCloseSideHash = Animator.StringToHash("RobotHeadCloseSide");
            _robotHeadIsClosedFront = Animator.StringToHash("RobotHeadIsClosedFront");
            _robotHeadIsClosedSide = Animator.StringToHash("RobotHeadIsClosedSide");
            
            // Body animations
            
            _idleAnimations = new Dictionary<(Direction, LevelGridManager.KeyType), int>()
            {
                { (Direction.Up, LevelGridManager.KeyType.None), Animator.StringToHash("IdleUp") },
                { (Direction.Down, LevelGridManager.KeyType.None), Animator.StringToHash("IdleDown") },
                { (Direction.Left, LevelGridManager.KeyType.None), Animator.StringToHash("IdleSide") },
                { (Direction.Right, LevelGridManager.KeyType.None), Animator.StringToHash("IdleSide") },

                { (Direction.Up, LevelGridManager.KeyType.Red), Animator.StringToHash("IdleUpWithRedKey") },
                { (Direction.Down, LevelGridManager.KeyType.Red), Animator.StringToHash("IdleDownWithRedKey") },
                { (Direction.Left, LevelGridManager.KeyType.Red), Animator.StringToHash("IdleSideWithRedKey") },
                { (Direction.Right, LevelGridManager.KeyType.Red), Animator.StringToHash("IdleSideWithRedKey") },

                { (Direction.Up, LevelGridManager.KeyType.Blue), Animator.StringToHash("IdleUpWithBlueKey") },
                { (Direction.Down, LevelGridManager.KeyType.Blue), Animator.StringToHash("IdleDownWithBlueKey") },
                { (Direction.Left, LevelGridManager.KeyType.Blue), Animator.StringToHash("IdleSideWithBlueKey") },
                { (Direction.Right, LevelGridManager.KeyType.Blue), Animator.StringToHash("IdleSideWithBlueKey") }
            };
            _idleClosedAnimations = new Dictionary<Direction, int>()
            {
                { Direction.Up, Animator.StringToHash("IdleUpClosed") },
                { Direction.Down, Animator.StringToHash("IdleDownClosed") },
                { Direction.Left, Animator.StringToHash("IdleSideClosed") },
                { Direction.Right, Animator.StringToHash("IdleSideClosed") }
            };
            _moveAnimations = new Dictionary<(Direction, LevelGridManager.KeyType), int>()
            {
                { (Direction.Up, LevelGridManager.KeyType.None), Animator.StringToHash("WalkUp") },
                { (Direction.Down, LevelGridManager.KeyType.None), Animator.StringToHash("WalkDown") },
                { (Direction.Left, LevelGridManager.KeyType.None), Animator.StringToHash("WalkSide") },
                { (Direction.Right, LevelGridManager.KeyType.None), Animator.StringToHash("WalkSide") },

                { (Direction.Up, LevelGridManager.KeyType.Red), Animator.StringToHash("WalkUpWithRedKey") },
                { (Direction.Down, LevelGridManager.KeyType.Red), Animator.StringToHash("WalkDownWithRedKey") },
                { (Direction.Left, LevelGridManager.KeyType.Red), Animator.StringToHash("WalkSideWithRedKey") },
                { (Direction.Right, LevelGridManager.KeyType.Red), Animator.StringToHash("WalkSideWithRedKey") },

                { (Direction.Up, LevelGridManager.KeyType.Blue), Animator.StringToHash("WalkUpWithBlueKey") },
                { (Direction.Down, LevelGridManager.KeyType.Blue), Animator.StringToHash("WalkDownWithBlueKey") },
                { (Direction.Left, LevelGridManager.KeyType.Blue), Animator.StringToHash("WalkSideWithBlueKey") },
                { (Direction.Right, LevelGridManager.KeyType.Blue), Animator.StringToHash("WalkSideWithBlueKey") }
            };
            _reverseMoveAnimations = new Dictionary<(Direction, LevelGridManager.KeyType), int>()
            {
                { (Direction.Up, LevelGridManager.KeyType.None), Animator.StringToHash("WalkUpReverse") },
                { (Direction.Down, LevelGridManager.KeyType.None), Animator.StringToHash("WalkDownReverse") },
                { (Direction.Left, LevelGridManager.KeyType.None), Animator.StringToHash("WalkSideReverse") },
                { (Direction.Right, LevelGridManager.KeyType.None), Animator.StringToHash("WalkSideReverse") }
            };
            _turnAnimations = new Dictionary<(Direction, Direction, LevelGridManager.KeyType), int>()
            {
                {
                    (Direction.Down, Direction.Left, LevelGridManager.KeyType.None),
                    Animator.StringToHash("TurnDownToSide")
                },
                {
                    (Direction.Down, Direction.Right, LevelGridManager.KeyType.None),
                    Animator.StringToHash("TurnDownToSide")
                },
                {
                    (Direction.Down, Direction.Left, LevelGridManager.KeyType.Blue),
                    Animator.StringToHash("TurnDownToSideWithBlueKey")
                },
                {
                    (Direction.Down, Direction.Right, LevelGridManager.KeyType.Blue),
                    Animator.StringToHash("TurnDownToSideWithBlueKey")
                },
                {
                    (Direction.Down, Direction.Left, LevelGridManager.KeyType.Red),
                    Animator.StringToHash("TurnDownToSideWithRedKey")
                },
                {
                    (Direction.Down, Direction.Right, LevelGridManager.KeyType.Red),
                    Animator.StringToHash("TurnDownToSideWithRedKey")
                },

                {
                    (Direction.Left, Direction.Down, LevelGridManager.KeyType.None),
                    Animator.StringToHash("TurnSideToDown")
                },
                {
                    (Direction.Right, Direction.Down, LevelGridManager.KeyType.None),
                    Animator.StringToHash("TurnSideToDown")
                },
                {
                    (Direction.Left, Direction.Down, LevelGridManager.KeyType.Blue),
                    Animator.StringToHash("TurnSideToDownWithBlueKey")
                },
                {
                    (Direction.Right, Direction.Down, LevelGridManager.KeyType.Blue),
                    Animator.StringToHash("TurnSideToDownWithBlueKey")
                },
                {
                    (Direction.Left, Direction.Down, LevelGridManager.KeyType.Red),
                    Animator.StringToHash("TurnSideToDownWithRedKey")
                },
                {
                    (Direction.Right, Direction.Down, LevelGridManager.KeyType.Red),
                    Animator.StringToHash("TurnSideToDownWithRedKey")
                },

                {
                    (Direction.Left, Direction.Up, LevelGridManager.KeyType.None), Animator.StringToHash("TurnSideToUp")
                },
                {
                    (Direction.Right, Direction.Up, LevelGridManager.KeyType.None),
                    Animator.StringToHash("TurnSideToUp")
                },
                {
                    (Direction.Left, Direction.Up, LevelGridManager.KeyType.Blue),
                    Animator.StringToHash("TurnSideToUpWithBlueKey")
                },
                {
                    (Direction.Right, Direction.Up, LevelGridManager.KeyType.Blue),
                    Animator.StringToHash("TurnSideToUpWithBlueKey")
                },
                {
                    (Direction.Left, Direction.Up, LevelGridManager.KeyType.Red),
                    Animator.StringToHash("TurnSideToUpWithRedKey")
                },
                {
                    (Direction.Right, Direction.Up, LevelGridManager.KeyType.Red),
                    Animator.StringToHash("TurnSideToUpWithRedKey")
                },

                {
                    (Direction.Up, Direction.Left, LevelGridManager.KeyType.None), Animator.StringToHash("TurnUpToSide")
                },
                {
                    (Direction.Up, Direction.Right, LevelGridManager.KeyType.None),
                    Animator.StringToHash("TurnUpToSide")
                },
                {
                    (Direction.Up, Direction.Left, LevelGridManager.KeyType.Blue),
                    Animator.StringToHash("TurnUpToSideWithBlueKey")
                },
                {
                    (Direction.Up, Direction.Right, LevelGridManager.KeyType.Blue),
                    Animator.StringToHash("TurnUpToSideWithBlueKey")
                },
                {
                    (Direction.Up, Direction.Left, LevelGridManager.KeyType.Red),
                    Animator.StringToHash("TurnUpToSideWithRedKey")
                },
                {
                    (Direction.Up, Direction.Right, LevelGridManager.KeyType.Red),
                    Animator.StringToHash("TurnUpToSideWithRedKey")
                }
            };
        }

        #region OnEnable/OnDisable

        private void OnEnable()
        {
            GameManager.BeamRobotIn += StartBeamSpawnEffect;
            GameManager.BeamRobotOut += StartBeamDespawnEffect;
        }

        private void OnDisable()
        {
            GameManager.BeamRobotIn -= StartBeamSpawnEffect;
            GameManager.BeamRobotOut -= StartBeamDespawnEffect;
        }
        
        #endregion
        
        public void SetSpriteSortingOrder(int sortingOrder)
        {
            bodySpriteRenderer.sortingOrder = sortingOrder;
            headSpriteRenderer.sortingOrder = sortingOrder + 1;
            headGateSpriteRenderer.sortingOrder = sortingOrder + 2;
            frontParticles.GetComponent<Renderer>().sortingOrder = sortingOrder + 1;
            backParticles.GetComponent<Renderer>().sortingOrder = sortingOrder - 1;
        }

        private void StartBeamSpawnEffect(float beamTime)
        {
            SoundPlayer.Instance.PlayBeamSpawn();

            DOVirtual.Color(beamTransparentColor, beamSolidColor, beamTime, value =>
            {
                bodySpriteRenderer.color = value;
            }).OnComplete(OpenHead);
            
            frontParticles.Play();
            backParticles.Play();
        }

        public void StartBeamDespawnEffect(float beamTime)
        {
            SoundPlayer.Instance.PlayBeamDespawn();
            DOVirtual.Color(beamSolidColor, beamTransparentColor, beamTime, value =>
            {
                bodySpriteRenderer.color = value;
            });
            frontParticles.Play();
            backParticles.Play();
        }

        #region Head

        public void SetHeadToClosed()
        {
            if (_direction == Direction.Down || _direction == Direction.Up)
            {
                headGateAnimator.CrossFade(_robotHeadIsClosedFront, 0, 0);   
            }else{
                headGateAnimator.CrossFade(_robotHeadIsClosedSide, 0, 0);
            }
        }

        public void OpenHead()
        {
            MovementToIdle();
            headSpriteRenderer.enabled = true;
            headGateSpriteRenderer.enabled = true;
            if (_direction == Direction.Down || _direction == Direction.Up)
            {
                headGateAnimator.CrossFade(_robotHeadOpenFrontHash, 0, 0);   
            }else{
                headGateAnimator.CrossFade(_robotHeadOpenSideHash, 0, 0);
            }
        }

        public void CloseHead()
        {
            SetToIdleClosed();
            headSpriteRenderer.enabled = false;
            headGateSpriteRenderer.enabled = false;
        }

        public void StartUp()
        {
            headAnimator.CrossFade(_robotStartUpHash, 0, 0);
        }

        public void ShutDown()
        {
            headAnimator.CrossFade(_robotShutDownHash, 0, 0);   
        }

        public void SetHeadSpriteToOff()
        {
            headAnimator.CrossFade(_robotOffHash, 0, 0);
        }

        #endregion

        #region Body

        public void SetCarryKeyType(LevelGridManager.KeyType keyType)
        {
            _keyState = keyType;
        }

        public void SetDirection(Direction direction)
        {
            _direction = direction;
        }

        public void Turn(Direction nextDirection)
        {
            if (nextDirection == Direction.Right)
                bodySpriteRenderer.flipX = true;
            if (nextDirection == Direction.Left)
                bodySpriteRenderer.flipX = false;
            
            var turnAnimation = _turnAnimations[(_direction, nextDirection, _keyState)];
            bodyAnimator.CrossFade(turnAnimation, 0, 0);
            _direction = nextDirection;
            Invoke(nameof(MovementToIdle), 0.6f);
        }

        public void GoForward()
        {
            var moveAnimation = _moveAnimations[(_direction, _keyState)];
            bodyAnimator.CrossFade(moveAnimation, 0, 0);
            Invoke(nameof(MovementToIdle), 0.6f);
        }

        public void GoReverse(float moveTime)
        {
            var reverseAnimation = _reverseMoveAnimations[(_direction, _keyState)];
            bodyAnimator.CrossFade(reverseAnimation, 0, 0);
            Invoke(nameof(MovementToIdle), moveTime);
        }

        private void MovementToIdle()
        {
            var idleAnimation = _idleAnimations[(_direction, _keyState)];
            bodyAnimator.CrossFade(idleAnimation, 0, 0);
        }

        public void UpdateSprite()
        {
            bodySpriteRenderer.flipX = _direction == Direction.Right;
            MovementToIdle();
        }

        public void Initialize()
        {
            bodySpriteRenderer.flipX = _direction == Direction.Right;
            SetToIdleClosed();
        }

        private void SetToIdleClosed()
        {
            var closedIdleAnimation = _idleClosedAnimations[_direction];
            bodyAnimator.CrossFade(closedIdleAnimation, 0, 0);
        }

        #endregion
    }
}