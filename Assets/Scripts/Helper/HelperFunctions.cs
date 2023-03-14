using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Helper
{
    public static class HelperFunctions 
    {
        public static bool CheckIfMouseIsOverObjectWithTag(string searchTag)
        {
            var raycastResults = GetRaycastResultsOnPosition(Input.mousePosition);
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
            var raycastResults = GetRaycastResultsOnPosition(Input.mousePosition);
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

        public static List<RaycastResult> GetRaycastResultsOnPosition(Vector2 position)
        {
            var pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = position;
            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            
            return raycastResults;
        }

        public static Vector2 GetMidpointOfVectors(Vector2 vector1, Vector2 vector2)
        {
            return (vector1 + vector2) / 2;
        }
    }
}
