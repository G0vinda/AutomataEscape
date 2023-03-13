using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils 
{
    public static Vector2 GetNextFaceDirection(Vector2 currentFaceDirection, Vector3 directionToOtherState)
    {
        var xBiggerThanY = Mathf.Abs(directionToOtherState.x) > Mathf.Abs(directionToOtherState.y);
        var xIsPositive = directionToOtherState.x > 0;
        var yIsPositive = directionToOtherState.y > 0;

        if (currentFaceDirection.Equals(Vector2.up))
        {
            if (xBiggerThanY)
            {
                if (yIsPositive)
                {
                    return Vector2.down;
                }
                return xIsPositive ? Vector2.left : Vector2.right;
            }
            
            if (yIsPositive)
            {
                return xIsPositive ? Vector2.right : Vector2.left;
            }
        }
        else if(currentFaceDirection.Equals(Vector2.left))
        {
            if (xBiggerThanY)
            {
                if (!xIsPositive)
                {
                    return yIsPositive ? Vector2.up : Vector2.down;
                }
            }
            else
            {
                if (!xIsPositive)
                {
                    return Vector2.right;
                }
                return yIsPositive ? Vector2.down : Vector2.up;
            }   
        }
        else if (currentFaceDirection.Equals(Vector2.down))
        {
            if (xBiggerThanY)
            {
                if (yIsPositive)
                {
                    return xIsPositive ? Vector2.left : Vector2.right;
                }
                return Vector2.up;
            }

            if (!yIsPositive)
            {
                return xIsPositive ? Vector2.right : Vector2.left;
            }
        }
        else if(currentFaceDirection.Equals(Vector2.right))
        {
            if (xBiggerThanY)
            {
                if (xIsPositive)
                {
                    return yIsPositive ? Vector2.up : Vector2.down;
                }
            }
            else
            {
                if (!xIsPositive)
                {
                    return yIsPositive ? Vector2.down : Vector2.up;
                }
                return Vector2.left;
            }
        }
        
        return Vector2.zero;
    }
}
