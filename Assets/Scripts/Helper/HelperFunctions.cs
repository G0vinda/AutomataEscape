using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Helper
{
    public static class HelperFunctions 
    {
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
