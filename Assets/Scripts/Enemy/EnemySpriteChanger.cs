using System;
using UI.Transition;
using UnityEngine;

namespace Enemy
{
    public class EnemySpriteChanger : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        
        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public void SetSpriteSortingOrder(int sortingOrder)
        {
            _spriteRenderer.sortingOrder = sortingOrder;
        }

        public void AdjustSpriteToDirection(Direction direction)
        {
            _spriteRenderer.flipX = direction == Direction.Left;
        }
    }
}