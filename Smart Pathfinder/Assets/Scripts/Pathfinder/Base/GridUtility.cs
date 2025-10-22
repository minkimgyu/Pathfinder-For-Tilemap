using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfinderForTilemap
{
    public static class GridUtility
    {
        // (�� �� �� ��), (�� �� �� ��) ��� ������ ���
        static readonly public Grid2D[] NearIndexes = new Grid2D[]  // �� �� �� �� �� �� �� �� �� ���
        {
            new Grid2D(-1, -1), new Grid2D(-1, 0), new Grid2D(-1, 1),

            new Grid2D(0, -1), new Grid2D(0, 1),

            new Grid2D(1, -1), new Grid2D(1, 0), new Grid2D(1, 1)
        };

        static readonly public int[] NearStraightIndexes = new int[] { 1, 3, 4, 6 };
        static readonly public int[] NearDiagonalIndexes = new int[] { 0, 2, 5, 7 };
    }
}