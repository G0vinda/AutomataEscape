using System;
using DG.Tweening;
using LevelGrid;
using UI;
using UI.Transition;
using UnityEngine;

namespace Robot
{
    public class SpriteChanger : MonoBehaviour
    {
        [Header("RobotSprites")]
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

        private SpriteRenderer _spriteRenderer;
        private Direction _direction;

        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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

        public void SetCarryKeyType(LevelGridManager.KeyType keyType)
        {
            switch (keyType)
            {
                case LevelGridManager.KeyType.None:
                    upSprite = upRobot;
                    sideSprite = sideRobot;
                    downSprite = downRobot;           
                    break;
                case LevelGridManager.KeyType.Blue:
                    upSprite = upRobotWithBlueKey;
                    sideSprite = sideRobotWithBlueKey;
                    downSprite = downRobotWithBlueKey;
                    break;
                case LevelGridManager.KeyType.Red:
                    upSprite = upRobotWithRedKey;
                    sideSprite = sideRobotWithRedKey;
                    downSprite = downRobotWithRedKey;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            UpdateSprite();
        }

        public void SetSpriteDirection(Direction direction)
        {
            _direction = direction;
        
            UpdateSprite();
        }

        private void UpdateSprite()
        {
            _spriteRenderer.flipX = false;
            switch (_direction)
            {
                case Direction.Up:
                    _spriteRenderer.sprite = upSprite;
                    break;
                case Direction.Right:
                    _spriteRenderer.sprite = sideSprite;
                    break;
                case Direction.Down:
                    _spriteRenderer.sprite = downSprite;
                    break;
                case Direction.Left:
                    _spriteRenderer.sprite = sideSprite;
                    _spriteRenderer.flipX = true;
                    break;
            }
        }

        public void SetSpriteSortingOrder(int sortingOrder)
        {
            _spriteRenderer.sortingOrder = sortingOrder;
            frontParticles.GetComponent<Renderer>().sortingOrder = sortingOrder + 1;
            backParticles.GetComponent<Renderer>().sortingOrder = sortingOrder - 1;
        }

        private void StartBeamSpawnEffect(float beamTime)
        {
            SoundPlayer.Instance.PlayBeamSpawn();
            DOVirtual.Color(beamTransparentColor, beamSolidColor, beamTime, value => _spriteRenderer.color = value);
            frontParticles.Play();
            backParticles.Play();
        }

        public void StartBeamDespawnEffect(float beamTime)
        {
            SoundPlayer.Instance.PlayBeamDespawn();
            DOVirtual.Color(beamSolidColor, beamTransparentColor, beamTime, value => _spriteRenderer.color = value);
            frontParticles.Play();
            backParticles.Play();
        }
    }
}