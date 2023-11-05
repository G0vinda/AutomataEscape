using System;
using System.Collections.Generic;
using UI;
using UI.Transition;
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

        public static Vector2 ToVector2(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => Vector2.up,
                Direction.Down => Vector2.down,
                Direction.Left => Vector2.left,
                Direction.Right => Vector2.right,
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

            throw new ArgumentException($"{vector} is not a direction vector");
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

        public static bool IsHorizontal(this Direction direction)
        {
            return direction is Direction.Left or Direction.Right;
        }

        public static bool IsVertical(this Direction direction)
        {
            return direction is Direction.Up or Direction.Down;
        }

        public static bool IsOpposite(this Direction dirA, Direction dirB)
        {
            return dirA == dirB.Opposite();
        }

        public static bool IsOrthogonal(this Direction dirA, Direction dirB)
        {
            return (dirA is Direction.Up or Direction.Down && dirB is Direction.Left or Direction.Right) ||
                   (dirA is Direction.Left or Direction.Right && dirB is Direction.Up or Direction.Down);
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

        public static List<Vector2Int> GetAdjacentCoordinates(this Vector2Int coordinates)
        {
            return new List<Vector2Int>()
            {
                coordinates + Vector2Int.up,
                coordinates + Vector2Int.right,
                coordinates + Vector2Int.down,
                coordinates + Vector2Int.left
            };
        }
    }
}
