using System;
using System.Collections;
using DG.Tweening;
using Helper;
using LevelGrid;
using UI.Transition;
using UnityEngine;
using UnityEngine.Serialization;

namespace Enemy
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        
        private Vector2Int _currentCoordinates;
        private Direction _currentDirection;
        private LevelGridManager _levelGridManager;
        private EnemySpriteChanger _spriteChanger;
        private int _walkingScanHash;
        private int _alarmHash;

        private void Awake()
        {
            _walkingScanHash = Animator.StringToHash("WalkingScan");
            _alarmHash = Animator.StringToHash("Alarm");
        }

        public void Initialize(Vector2Int coordinates, Direction direction, LevelGridManager levelGridManager)
        {
            _currentCoordinates = coordinates;
            _currentDirection = direction;
            _levelGridManager = levelGridManager;
            _spriteChanger = GetComponent<EnemySpriteChanger>();
            _spriteChanger.AdjustSpriteToDirection(_currentDirection);
            _spriteChanger.SetSpriteSortingOrder(LevelGridManager.GetSpriteSortingOrderFromCoordinates(_currentCoordinates));
        }

        public Vector2Int GetCoordinates()
        {
            return _currentCoordinates;
        } 

        public void Move()
        {
            if (_levelGridManager.CheckIfWayIsBlocked(_currentCoordinates, _currentDirection))
            {
                _currentDirection = _currentDirection.Opposite();
                _spriteChanger.AdjustSpriteToDirection(_currentDirection);
            }

            _currentCoordinates += _currentDirection.ToVector2Int();
            var moveTime = 0.4f;
            transform.DOMove(_levelGridManager.Grid[_currentCoordinates].transform.position, moveTime).SetEase(Ease.InOutSine);
            animator.CrossFade(_walkingScanHash, 0, 0);
            GameManager.Instance.CheckForEnemyCollision();
        }

        public void StartAlarm()
        {
            animator.CrossFade(_alarmHash, 0, 0);
        }
    }
}