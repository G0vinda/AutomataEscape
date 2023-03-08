using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Helper
{
    public static class HelperFunctions 
    {
        public static bool CheckIfMouseAreaIsOverFreeAreaWithTag(float areaSize, string areaTag, string blockerTag)
        {
            var mousePosition = Input.mousePosition;
            var yPositionStep = areaSize * 2 / 3;
            var xPosition = mousePosition.x - areaSize;
            var yPosition = mousePosition.y - areaSize;
            for (var i = 0; i < 4; i++)
            {
                if (!CheckIfAreaRowIsOverFreeAreaWithTag(new Vector2(xPosition, yPosition), areaSize, areaTag, blockerTag))
                {
                    return false;
                }
                yPosition += + yPositionStep * i;
            }

            return true;
        }

        private static bool CheckIfAreaRowIsOverFreeAreaWithTag(Vector2 startPos, float areaSize, string areaTag,
            string blockerTag)
        {
            var xPositionStep = areaSize * 2 / 3;
            for (var i = 0; i < 4; i++)
            {
                if (!CheckIfPositionIsOverFreeAreaWithTag(startPos + new Vector2(i * xPositionStep, 0), areaTag,
                    blockerTag))
                {
                    return false;
                }
            }
            return true;
        }
        
        public static bool CheckIfMousePositionIsOverFreeAreaWithTag(string areaTag, string blockerTag)
        {
            return CheckIfPositionIsOverFreeAreaWithTag(Input.mousePosition, areaTag, blockerTag);
        }

        private static bool CheckIfPositionIsOverFreeAreaWithTag(Vector2 position, string areaTag, string blockerTag)
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            var rayCastResults = GetPositionRaycastResults(position, pointerEventData);
            var positionIsOverArea = false;
            var positionIsOverBlocker = false;
            foreach (var rayCastResult in rayCastResults)
            {
                if (rayCastResult.gameObject.CompareTag(areaTag))
                {
                    positionIsOverArea = true;
                }
                else if(rayCastResult.gameObject.CompareTag(blockerTag))
                {
                    positionIsOverBlocker = true;
                }
            }
            Debug.Log(positionIsOverArea && !positionIsOverBlocker ? "Position is available" : "Position is not available");
            return positionIsOverArea && !positionIsOverBlocker;
        }
        
        public static bool CheckIfMouseIsOverObjectWithTag(string searchTag)
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            var raycastResults = GetPositionRaycastResults(Input.mousePosition, pointerEventData);
            foreach (var raycastResult in raycastResults)
            {
                if (raycastResult.gameObject.CompareTag(searchTag))
                {
                    return true;
                }
            }

            return false;
        }

        public static T CheckIfMouseIsOverObjectWithComponent<T>()
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            var raycastResults = GetPositionRaycastResults(Input.mousePosition, pointerEventData);
            foreach (var raycastResult in raycastResults)
            {
                var resultParent = raycastResult.gameObject.transform.parent;
                if (resultParent.gameObject.TryGetComponent(out T outComponent))
                {
                    return outComponent;
                }
            }
            
            return default;
        }

        private static List<RaycastResult> GetPositionRaycastResults(Vector2 position, PointerEventData pointerEventData)
        {
            pointerEventData.position = position;
            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            
            // Debug.Log($"Number of raycast results: {raycastResults.Count}");
            // foreach (var raycastResult in raycastResults)
            // {
            //     Debug.Log($"resultName:{raycastResult.gameObject.name}");
            // }
            return raycastResults;
        }

        public static List<GameObject> CheckSquareOutlineForTransitionLines(float lineWidth, float squareSize, Vector2 centerPosition)
        {
            var checkDistance = lineWidth - 1;
            var raycastsPerSide = (int)Mathf.Floor(squareSize / checkDistance);
            var transitionLineTag = "TransitionLine";

            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            List<RaycastResult> allRaycastResults = new List<RaycastResult>();
            
            // Top side
            var raycastPosition = centerPosition + new Vector2(-squareSize * 0.5f, squareSize * 0.5f);
            var offsetVector = new Vector2(checkDistance, 0);
            for (var i = 0; i < raycastsPerSide; i++)
            {
                allRaycastResults.AddRange(GetPositionRaycastResults(raycastPosition + offsetVector * i,
                    pointerEventData));
            }
            
            // Right side
            raycastPosition = centerPosition + new Vector2(squareSize * 0.5f, squareSize * 0.5f);
            offsetVector = new Vector2(0, -checkDistance);
            for (var i = 0; i < raycastsPerSide; i++)
            {
                allRaycastResults.AddRange(GetPositionRaycastResults(raycastPosition + offsetVector * i,
                    pointerEventData));
            }
            
            // Bottom side
            raycastPosition = centerPosition + new Vector2(squareSize * 0.5f, -squareSize * 0.5f);
            offsetVector = new Vector2(-checkDistance, 0);
            for (var i = 0; i < raycastsPerSide; i++)
            {
                allRaycastResults.AddRange(GetPositionRaycastResults(raycastPosition + offsetVector * i,
                    pointerEventData));
            }
            
            // Left side
            raycastPosition = centerPosition + new Vector2(-squareSize * 0.5f, -squareSize * 0.5f);
            offsetVector = new Vector2(0, checkDistance);
            for (var i = 0; i < raycastsPerSide; i++)
            {
                allRaycastResults.AddRange(GetPositionRaycastResults(raycastPosition + offsetVector * i,
                    pointerEventData));
            }

            var foundGameObjects = allRaycastResults.ToGameObjects().Distinct().ToList();

            return foundGameObjects;
        }

        private static void TestDestroyLineIfPointerIsOver()
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        }

        public static List<RaycastResult> LineClickTest()
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            return GetPositionRaycastResults(Input.mousePosition, pointerEventData);
        }
    }
}
