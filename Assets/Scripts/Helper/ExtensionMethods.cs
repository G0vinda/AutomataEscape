using System;
using UI;
using UnityEngine;

namespace Helper
{
    public static class ExtensionMethods 
    {
        public static Vector2Int ToVector2Int(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => Vector2Int.up,
                Direction.Down => Vector2Int.down,
                Direction.Left => Vector2Int.left,
                Direction.Right => Vector2Int.right,
                _ => throw new ArgumentException()
            };
        }

        public static Direction ToDirection(this Vector2Int vector)
        {
            if (vector == Vector2Int.up)
            {
                return Direction.Up;
            }
            if (vector == Vector2Int.down)
            {
                return Direction.Down;
            }
            if (vector == Vector2Int.left)
            {
                return Direction.Left;
            }
            if (vector == Vector2Int.right)
            {
                return Direction.Right;
            }

            throw new ArgumentException();
        }

        public static Direction ToDirection(this Vector2 vector)
        {
            if (Mathf.Approximately(vector.y, 0))
            {
                switch (vector.x)
                {
                    case > 0 :
                        return Direction.Right;
                    case < 0 :
                        return Direction.Left;
                    default:
                        throw new ArgumentException();
                }   
            }
            
            if (Mathf.Approximately(vector.x, 0))
            {
                switch (vector.y)
                {
                    case > 0 :
                        return Direction.Up;
                    case < 0 :
                        return Direction.Down;
                    default:
                        throw new ArgumentException();
                }   
            }
            
            throw new ArgumentException();
        }

        public static bool IsOpposite(this Direction dirA, Direction dirB)
        {
            return dirA == Direction.Up && dirB == Direction.Down ||
                   dirA == Direction.Down && dirB == Direction.Up ||
                   dirA == Direction.Left && dirB == Direction.Right ||
                   dirA == Direction.Right && dirB == Direction.Left;
        }

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

        public static Quaternion ToZRotation(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => Quaternion.identity, // Object are rotated upwards by default
                Direction.Down => Quaternion.Euler(0, 0, 180),
                Direction.Left => Quaternion.Euler(0, 0, 90),
                Direction.Right => Quaternion.Euler(0, 0, -90),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static float SumOfElements(this Vector2 vector)
        {
            return Vector2.Dot(vector, Vector2.one);
        }

        public static bool IsInsideSquare(this Vector2 position, Vector2 squareBottomLeft, float squareSize)
        {
            return !(position.x < squareBottomLeft.x || position.x > squareBottomLeft.x + squareSize ||
                     position.y < squareBottomLeft.y || position.y > squareBottomLeft.y + squareSize);
        }
    }
}
