using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace PathfinderForTilemap
{
    public static class Heuristic
    {
        static readonly float SQRT_2 = Mathf.Sqrt(2);
        static readonly float SQRT_2_MINUS_1 = Mathf.Sqrt(2) - 1.0f;

        public static int GetOctileDistance(Grid2D start, Grid2D goal)
        {
            int heuristic;
            int rowDist = Mathf.Abs(goal.Row - start.Row);
            int columnDist = Mathf.Abs(goal.Column - start.Column); // 휴리스틱 값이 음수가 나오는 문제 발생

            heuristic = (int)(Mathf.Max(rowDist, columnDist) + SQRT_2_MINUS_1 * Mathf.Min(rowDist, columnDist));

            return heuristic;
        }

        public static float GetEuclideanDistance(Grid2D start, Grid2D goal)
        {
            int rowDist = goal.Row - start.Row;
            int columnDist = goal.Column - start.Column;
            return Mathf.Sqrt(rowDist * rowDist + columnDist * columnDist);
        }

        public static float GetManhattanDistance(Grid2D start, Grid2D goal)
        {
            int rowDist = Mathf.Abs(goal.Row - start.Row);
            int columnDist = Mathf.Abs(goal.Column - start.Column);
            return rowDist + columnDist;
        }
    }
}
