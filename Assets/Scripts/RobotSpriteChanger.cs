using System;
using UI;
using UnityEngine;
using UnityEngine.PlayerLoop;


public class RobotSpriteChanger : MonoBehaviour
{
    [SerializeField] private Sprite upRobot;
    [SerializeField] private Sprite sideRobot;
    [SerializeField] private Sprite downRobot;

    [SerializeField] private Sprite upRobotWithBlueKey;
    [SerializeField] private Sprite sideRobotWithBlueKey;
    [SerializeField] private Sprite downRobotWithBlueKey;

    [SerializeField] private Sprite upRobotWithRedKey;
    [SerializeField] private Sprite sideRobotWithRedKey;
    [SerializeField] private Sprite downRobotWithRedKey;
    
    private Sprite upSprite;
    private Sprite sideSprite;
    private Sprite downSprite;

    private SpriteRenderer _spriteRenderer;
    private Direction _direction;

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void SetCarryKeyType(GridManager.KeyType keyType)
    {
        switch (keyType)
        {
            case GridManager.KeyType.None:
                upSprite = upRobot;
                sideSprite = sideRobot;
                downSprite = downRobot;           
                break;
            case GridManager.KeyType.Blue:
                upSprite = upRobotWithBlueKey;
                sideSprite = sideRobotWithBlueKey;
                downSprite = downRobotWithBlueKey;
                break;
            case GridManager.KeyType.Red:
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
    }
}