using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LevelGrid;
using UI.Transition;
using UnityEngine;

namespace Robot
{
    public class SpriteChanger : MonoBehaviour
    {
        [Header("Animators")] 
        [SerializeField] private Animator headGateAnimator;
        [SerializeField] private Animator headAnimator;
        [SerializeField] private Animator faceAnimator;
        [SerializeField] private Animator bodyAnimator;

        [Header("SpriteRenderers")] 
        [SerializeField] private SpriteRenderer headGateSpriteRenderer;
        [SerializeField] private SpriteRenderer headSpriteRenderer;
        [SerializeField] private SpriteRenderer facePlateRenderer;
        [SerializeField] private SpriteRenderer bodySpriteRenderer;

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

        private int _headStartUpHash;
        private int _headShutDownHash;
        private int _headOffHash;

        private int _faceStartUpHash;
        private int _faceShutDownHash;
        private int _faceOffHash;
        private int _faceOnHash;
        
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
        private Dictionary<(Direction, LevelGridManager.KeyType), int> _grabAnimations;
        private Dictionary<(Direction, LevelGridManager.KeyType), int> _dropAnimations;

        private void Awake()
        {
            // Head animations

            _headStartUpHash = Animator.StringToHash("RobotStartUp");
            _headShutDownHash = Animator.StringToHash("RobotShutDown");
            _headOffHash = Animator.StringToHash("RobotOff");

            _faceStartUpHash = Animator.StringToHash("RobotStartUp");
            _faceShutDownHash = Animator.StringToHash("RobotShutDown");
            _faceOffHash = Animator.StringToHash("RobotOff");
            _faceOnHash = Animator.StringToHash("RobotOn");
            
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
            _grabAnimations = new Dictionary<(Direction, LevelGridManager.KeyType), int>()
            {
                { (Direction.Up, LevelGridManager.KeyType.Red), Animator.StringToHash("GrabBackWithRedKey") },
                { (Direction.Up, LevelGridManager.KeyType.Blue), Animator.StringToHash("GrabBackWithBlueKey") },
                { (Direction.Left, LevelGridManager.KeyType.Red), Animator.StringToHash("GrabSideWithRedKey") },
                { (Direction.Left, LevelGridManager.KeyType.Blue), Animator.StringToHash("GrabSideWithBlueKey") },
                { (Direction.Right, LevelGridManager.KeyType.Red), Animator.StringToHash("GrabSideWithRedKey") },
                { (Direction.Right, LevelGridManager.KeyType.Blue), Animator.StringToHash("GrabSideWithBlueKey") },
                { (Direction.Down, LevelGridManager.KeyType.Red), Animator.StringToHash("GrabFrontWithRedKey") },
                { (Direction.Down, LevelGridManager.KeyType.Blue), Animator.StringToHash("GrabFrontWithBlueKey") },
            };
            _dropAnimations = new Dictionary<(Direction, LevelGridManager.KeyType), int>()
            {
                { (Direction.Up, LevelGridManager.KeyType.Red), Animator.StringToHash("DropBackWithRedKey") },
                { (Direction.Up, LevelGridManager.KeyType.Blue), Animator.StringToHash("DropBackWithBlueKey") },
                { (Direction.Left, LevelGridManager.KeyType.Red), Animator.StringToHash("DropSideWithRedKey") },
                { (Direction.Left, LevelGridManager.KeyType.Blue), Animator.StringToHash("DropSideWithBlueKey") },
                { (Direction.Right, LevelGridManager.KeyType.Red), Animator.StringToHash("DropSideWithRedKey") },
                { (Direction.Right, LevelGridManager.KeyType.Blue), Animator.StringToHash("DropSideWithBlueKey") },
                { (Direction.Down, LevelGridManager.KeyType.Red), Animator.StringToHash("DropFrontWithRedKey") },
                { (Direction.Down, LevelGridManager.KeyType.Blue), Animator.StringToHash("DropFrontWithBlueKey") },
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
            headSpriteRenderer.enabled = false;
            headGateSpriteRenderer.enabled = false;
            SetToIdleClosed();
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

        public void SetHeadToOpen()
        {
            headGateSpriteRenderer.enabled = false;
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
            headGateSpriteRenderer.enabled = true;
            if (_direction == Direction.Down || _direction == Direction.Up)
            {
                headGateAnimator.CrossFade(_robotHeadCloseFrontHash, 0, 0);
            }else{
                headGateAnimator.CrossFade(_robotHeadCloseSideHash, 0, 0);
            }
        }

        public void StartUp()
        {
            headAnimator.CrossFade(_headStartUpHash, 0, 0);
        }

        public void ShutDown()
        {
            headSpriteRenderer.enabled = true;
            headAnimator.CrossFade(_headShutDownHash, 0, 0);
        }

        public void SetHeadSpriteToOff()
        {
            headAnimator.CrossFade(_headOffHash, 0, 0);
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

        public void GrabKey(LevelGridManager.KeyType keyType, Action pickUpAction)
        {
            headSpriteRenderer.enabled = false;
            _keyState = keyType;
            var grabAnimation = _grabAnimations[(_direction, keyType)];
            var animationToFloorTime = 22f / 60f;
            bodyAnimator.CrossFade(grabAnimation, 0, 0);
            StartCoroutine(PlayDelayedAction(pickUpAction, animationToFloorTime));
            Invoke(nameof(MovementToIdle), 0.6f);
        }

        public void DropKey(Action dropAction)
        {
            headSpriteRenderer.enabled = false;
            var dropAnimation = _dropAnimations[(_direction, _keyState)];
            var animationToFloorTime = 18f / 60f;
            bodyAnimator.CrossFade(dropAnimation, 0, 0);
            _keyState = LevelGridManager.KeyType.None;
            StartCoroutine(PlayDelayedAction(dropAction, animationToFloorTime));
            Invoke(nameof(MovementToIdle), 0.6f);
        }
        
        private IEnumerator PlayDelayedAction(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action();
        }

        public void GoForward()
        {
            headSpriteRenderer.enabled = false;
            var moveAnimation = _moveAnimations[(_direction, _keyState)];
            bodyAnimator.CrossFade(moveAnimation, 0, 0);
            Invoke(nameof(MovementToIdle), 0.6f);
        }

        private void MovementToIdle()
        {
            headSpriteRenderer.enabled = true;
            
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