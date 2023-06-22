using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LevelGrid;
using UnityEngine;

public class AnimationTest : MonoBehaviour
{
    [SerializeField] private Transform[] floorTiles;
    
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private int _walkingHash;
    private WaitForSeconds _wait = new WaitForSeconds(0.8f);
    private int _stepCount;
    private float _moveTime = 0.6f;
    private int _moveDirection;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        _walkingHash = Animator.StringToHash("walking");
        _stepCount = 0;
        _moveDirection = 1;
    }

    private void Start()
    {
        StartCoroutine(GoRoutine());
    }

    private IEnumerator GoRoutine()
    {
        while (true)
        {
            if (_stepCount == floorTiles.Length - 1)
            {
                _moveDirection = -1;
                _spriteRenderer.flipX = true;
            }else if (_stepCount == 0 && _moveDirection == -1)
            {
                _moveDirection = 1;
                _spriteRenderer.flipX = false;
            }

            _stepCount += _moveDirection;
            var goalTile = floorTiles[_stepCount];
            
            _animator.SetBool(_walkingHash, true);
            transform.DOMove(goalTile.position, _moveTime).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                _animator.SetBool(_walkingHash, false);
            });
            yield return _wait;
        }
    }
}
