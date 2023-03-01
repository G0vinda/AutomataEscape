using System;
using System.Collections.Generic;
using Helper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StateUIElement : MonoBehaviour
    {
        [SerializeField] protected TransitionPlug defaultTransitionPlugPrefab;
        [SerializeField] protected RectTransform[] transitionSlotTransforms;
        [SerializeField] protected float maxSlotMouseAreaWidth;
        [SerializeField] protected float maxSlotMouseAreaHeight;
        
        public int AssignedId { get; set; }
        
        protected List<int> _emptySlotIds;
        protected StateChartUIManager UIManager;

        private void Start()
        {
            AssignedId = 0;
            UIManager = GameManager.Instance.GetStateChartUIManager();
            SetupEmptySlotIds();
            AddDefaultTransitionPlugToState();
        }

        protected void SetupEmptySlotIds()
        {
            _emptySlotIds = new List<int>();
            for (int i = 0; i < 12; i++)
            {
                _emptySlotIds.Add(i);    
            }
        }

        protected virtual void AddDefaultTransitionPlugToState()
        {
            var lastSlotId = transitionSlotTransforms.Length - 1;
            var newPlug = Instantiate(defaultTransitionPlugPrefab,
                transitionSlotTransforms[lastSlotId].position,
                transitionSlotTransforms[lastSlotId].rotation,
                transform);
            newPlug.GetLineTransform().rotation = Quaternion.identity;
            _emptySlotIds.Remove(lastSlotId);
        }
        
        public void MoveTransitionPlugToSlot(TransitionPlug plug, int newSlotId)
        {
            var plugTransform = plug.GetComponent<RectTransform>();
            plugTransform.position = transitionSlotTransforms[newSlotId].position;
            plugTransform.rotation = transitionSlotTransforms[newSlotId].rotation;
            plug.GetLineTransform().rotation = Quaternion.identity;

            SetupEmptySlotIds(); // Set all slots to empty
            _emptySlotIds.Remove(newSlotId);
        }
        
        public int IsPosInRangeOfEmptySlot(Vector3 pos)
        {
            foreach (var emptySlotId in _emptySlotIds)
            {
                var emptySlotTransform = transitionSlotTransforms[emptySlotId];
                var emptySlotPos = emptySlotTransform.position;
                if (Vector3.Distance(pos, emptySlotPos) > maxSlotMouseAreaWidth + maxSlotMouseAreaHeight)
                    continue;

                var emptySlotDir = emptySlotTransform.ZRotToDir();

                if (emptySlotDir.Equals(Vector2.zero))
                {
                    Debug.LogError("Couldn't read direction of empty slot.");
                    return -1;
                }

                float xMin, xMax, yMin, yMax;
                if (emptySlotDir.Equals(Vector2.left))
                {
                    xMin = emptySlotPos.x - maxSlotMouseAreaHeight;
                    xMax = emptySlotPos.x;
                    yMin = emptySlotPos.y - maxSlotMouseAreaWidth * 0.5f;
                    yMax = emptySlotPos.y + maxSlotMouseAreaWidth * 0.5f;
                } else if (emptySlotDir.Equals(Vector2.right))
                {
                    xMin = emptySlotPos.x;
                    xMax = emptySlotPos.x + maxSlotMouseAreaHeight;
                    yMin = emptySlotPos.y - maxSlotMouseAreaWidth * 0.5f;
                    yMax = emptySlotPos.y + maxSlotMouseAreaWidth * 0.5f;
                } else if (emptySlotDir.Equals(Vector2.down))
                {
                    xMin = emptySlotPos.x - maxSlotMouseAreaWidth * 0.5f;
                    xMax = emptySlotPos.x + maxSlotMouseAreaWidth * 0.5f;
                    yMin = emptySlotPos.y - maxSlotMouseAreaHeight;
                    yMax = emptySlotPos.y;
                } else // Vector2.up
                {
                    xMin = emptySlotPos.x - maxSlotMouseAreaWidth * 0.5f;
                    xMax = emptySlotPos.x + maxSlotMouseAreaWidth * 0.5f;
                    yMin = emptySlotPos.y;
                    yMax = emptySlotPos.y + maxSlotMouseAreaHeight;;
                }

                if (IsPosInSquare(pos, xMin, yMin, xMax, yMax))
                    return emptySlotId;
            }

            return -1;
        }
        
        protected bool IsPosInSquare(Vector3 pos, float xMin, float yMin, float xMax, float yMax)
        {
            var x = pos.x;
            var y = pos.y;
            if (x >= xMin && x <= xMax)
            {
                if (y >= yMin && y <= yMax)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
