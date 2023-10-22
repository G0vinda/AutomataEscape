using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI.State;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CreateLoopHint : MonoBehaviour
    {
        [SerializeField] private Image hintElementPrefab;
        [SerializeField] private Image arrowHintElementPrefab;
        [SerializeField] private float elementSpawnTime;
        [SerializeField] private float highlightEffectTime;

        private List<Image> _hintElements = new ();
        private float _subCellSize;
        private Vector2 _statePosition;
        private WaitForSeconds _spawnWait;
        private bool _hide;

        private void Awake()
        {
            _spawnWait = new WaitForSeconds(elementSpawnTime);
        }

        #region OnEnable/OnDisable

        private void OnEnable()
        {
            InputManager.StateElementSelected += HandleInputDetected;
            InputManager.DragEnded += HandleInputEndDetected;
        }
        
        private void OnDisable()
        {
            InputManager.StateElementSelected -= HandleInputDetected;
            InputManager.DragEnded -= HandleInputEndDetected;
        }

        #endregion

        public void StartDraw(float subCellSize, Vector2 statePosition, bool drawUpwards)
        {
            _subCellSize = subCellSize;
            _statePosition = statePosition;
            
            if(drawUpwards)
                StartCoroutine(DrawHintUpwards());
        }

        private IEnumerator DrawHintUpwards()
        {
            CreateHintElement(hintElementPrefab, Vector2.up, Quaternion.identity);
            yield return _spawnWait;

            CreateHintElement(hintElementPrefab, Vector2.up, Quaternion.Euler(0, 0, -90));
            yield return _spawnWait;
            
            CreateHintElement(hintElementPrefab, Vector2.right, Quaternion.Euler(0, 0, -90));
            yield return _spawnWait;
            
            CreateHintElement(hintElementPrefab, Vector2.right, Quaternion.Euler(0, 0, -180));
            yield return _spawnWait;
            
            CreateHintElement(hintElementPrefab, Vector2.down, Quaternion.Euler(0, 0, -180));
            yield return _spawnWait;
            
            CreateHintElement(arrowHintElementPrefab, Vector2.down, Quaternion.Euler(0, 0, -90));
            yield return _spawnWait;
        }

        private void CreateHintElement(Image elementPrefab, Vector2 direction, Quaternion rotation)
        {
            var offsetLength = _subCellSize * 2;
            var previousPosition = _hintElements.Count > 0 ? (Vector2)_hintElements.Last().transform.position : _statePosition;
            var spawnPosition = previousPosition + direction * offsetLength;
            var newHintElement = Instantiate(elementPrefab, spawnPosition, rotation, transform);
            newHintElement.gameObject.SetActive(!_hide);
            _hintElements.Add(newHintElement);
        }
        
        
        private void HandleInputDetected(StateUIElement obj)
        {
            _hide = true;
            foreach (var hintElement in _hintElements)
            {
                hintElement.gameObject.SetActive(false);
            }
        }
        
        private void HandleInputEndDetected()
        {
            _hide = false;
            foreach (var hintElement in _hintElements)
            {
                hintElement.gameObject.SetActive(true);
            }
        }
    }
}