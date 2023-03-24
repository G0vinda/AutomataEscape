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

        public static Direction Turn(this Direction direction, bool turnClockwise)
        {
            if (turnClockwise)
            {
                return direction == (Direction)3 ? 0 : direction + 1;   
            }

            return direction == 0 ? (Direction)3 : direction - 1;
        }

        public static Direction Opposite(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static bool IsOpposite(this Direction dirA, Direction dirB)
        {
            return dirA == dirB.Opposite();
        }

        public static Quaternion ToZRotation(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => Quaternion.identity, // Objects are rotated upwards by default
                Direction.Down => Quaternion.Euler(0, 0, 180),
                Direction.Left => Quaternion.Euler(0, 0, 90),
                Direction.Right => Quaternion.Euler(0, 0, -90),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static bool IsInsideSquare(this Vector2 position, Vector2 squareBottomLeft, float squareSize)
        {
            return !(position.x < squareBottomLeft.x || position.x > squareBottomLeft.x + squareSize ||
                     position.y < squareBottomLeft.y || position.y > squareBottomLeft.y + squareSize);
        }

        public static bool IsDefault<T>(this T value)
        {
            return value.Equals(default(T));
        }
    }
}
