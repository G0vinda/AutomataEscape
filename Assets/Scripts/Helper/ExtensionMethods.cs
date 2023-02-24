using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

namespace Helper
{
    public static class ExtensionMethods 
    {
        public static Vector2 ZRotToDir(this RectTransform transform)
        {
            var rotation = transform.eulerAngles.z;
            if (rotation < 0)
                rotation += 360;
            else if (rotation > 360)
                rotation -= 360;
            
            if (Math.Abs(rotation - 0) < float.Epsilon)
                return Vector2.up;
            
            if (Math.Abs(rotation - 90) < float.Epsilon)
                return Vector2.left;
            
            if (Math.Abs(rotation - 180) < float.Epsilon)
                return Vector2.down;
            
            if (Math.Abs(rotation - 270) < float.Epsilon)
                return Vector2.right;

            return Vector2.zero;
        }

        public static Quaternion DirToZRot(this Vector2 direction)
        {
            if (direction == Vector2.up)
                return Quaternion.identity;
            
            if(direction == Vector2.down)
                return Quaternion.Euler(0, 0, 180);
            
            if(direction == Vector2.right)
                return Quaternion.Euler(0, 0, -90);
            
            if(direction == Vector2.left)
                return Quaternion.Euler(0, 0, 90);

            Debug.LogError("Invalid direction input on DirToZRot");
            return Quaternion.identity;
        }

        public static float SumOfElements(this Vector2 vector)
        {
            return Vector2.Dot(vector, Vector2.one);
        }
    }
}
