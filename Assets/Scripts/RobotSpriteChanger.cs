using System;
using UI;
using UnityEngine;


public class RobotSpriteChanger : MonoBehaviour
{
    [SerializeField] private Sprite upRobot;
    [SerializeField] private Sprite sideRobot;
    [SerializeField] private Sprite downRobot;

    private SpriteRenderer _spriteRenderer;
    private Direction _previousDirection;
    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void SetSprite(Direction direction)
    {
        if (_previousDirection == Direction.Right)
            _spriteRenderer.flipX = false;
        switch (direction)
        {
            case Direction.Up:
                _spriteRenderer.sprite = upRobot;
                break;
            case Direction.Right:
                _spriteRenderer.sprite = sideRobot;
                _spriteRenderer.flipX = true;
                break;
            case Direction.Down:
                _spriteRenderer.sprite = downRobot;
                break;
            case Direction.Left:
                _spriteRenderer.sprite = sideRobot;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _previousDirection = direction;
    }
}
