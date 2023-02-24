using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Helper
{
    public static class HelperFunctions 
    {
        public static bool CheckIfMouseIsOverObjectWithTag(string searchTag)
        {
            var raycastResults = GetMouseOverRaycastResults();
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
            var raycastResults = GetMouseOverRaycastResults();
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

        private static List<RaycastResult> GetMouseOverRaycastResults()
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.mousePosition;
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            
            return raycastResults;
        }
    }
}
