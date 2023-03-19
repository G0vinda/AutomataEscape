using System;
using Helper;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

namespace UI
{
    public class TransitionPlug : MonoBehaviour, IPointerDownHandler
    {
        public StateChartManager.TransitionCondition transitionCondition;
        public StateUIElement connectedState;
        
        private RectTransform _rectTransform;
        private Image _image;
        private Vector2 _plugDir;
        private Vector3 _plugOuterPos;
        private bool _isBeingDragged;
        private Vector3 _mousePosition;
        private bool _isConnectedToOtherState;
        private int _connectedSlotId;

        private void Awake()
        {
            connectedState = GetComponentInParent<StateUIElement>();
            Debug.Log($"connectedState is: {connectedState.name}");
            _rectTransform = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
        }

        private void Start()
        {
            transform.SetAsFirstSibling();
            UpdateVariablesOnNewPos();
        }

        public void Initialize(TransitionUIData uiData)
        {
            _image.color = uiData.color;
            transitionCondition = uiData.condition;
        }

        public int GetConnectedSlotId()
        {
            return _connectedSlotId;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Debug.Log("Transition plug is clicked");
            // _isBeingDragged = true;
            // if (_isConnectedToOtherState)
            // {
            //     GameManager.Instance.GetUIManager().RemoveTransitionByPlug(this);
            //     transitionLineDrawer.ClearLine();
            //     _isConnectedToOtherState = false;
            // }
        }

        private void Update()
        {
            // if (!_isBeingDragged)
            //     return;
            //
            // if (Input.GetMouseButtonUp(0))
            // {
            //     _isBeingDragged = false;
            //     return;
            // }
            //
            // _mousePosition = Input.mousePosition;
            // if(MovePlugIfEmptySlotInRange())
            //     return;
            //
            // var posDiff = _mousePosition - _plugOuterPos;
            // var distance = (posDiff * _plugDir).SumOfElements();
            // if (transitionLineDrawer.StartDrawingIfInRange(distance, _plugDir))
            // {
            //     _isBeingDragged = false;
            // }
        }

        private bool MovePlugIfEmptySlotInRange()
        {
            int emptySlotId = connectedState.IsPositionInRangeOfEmptySlot(_mousePosition);
            if (emptySlotId >= 0)
            {
                connectedState.MoveTransitionPlugToSlot(this, emptySlotId);
                UpdateVariablesOnNewPos();
                return true;
            }

            return false;
        }

        public Transform GetLineTransform()
        {
            return transform.GetChild(0);
        }

        private void UpdateVariablesOnNewPos()
        {
            _plugDir = _rectTransform.ZRotToDir();
            _plugOuterPos = _rectTransform.position + (Vector3)_plugDir * _rectTransform.sizeDelta.y;
        }

        public void DisconnectLine()
        {
            // _isConnectedToOtherState = false;
            // transitionLineDrawer.ClearLine();
        }

        public void OnTransitionConnected(StateUIPlaceElement otherState, int connectedSlotId)
        {
            _isConnectedToOtherState = true;
            _connectedSlotId = connectedSlotId;
            GameManager.Instance.GetUIManager().HandleNewTransitionConnected(connectedState,
                otherState, this);
        }
    }
}
