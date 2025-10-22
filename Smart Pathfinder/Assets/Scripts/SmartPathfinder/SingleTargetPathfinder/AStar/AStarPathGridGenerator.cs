using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfinderForTilemap
{
    public class AStarPathGridGenerator : BaseAStarPathGridGenerator
    {
        protected override void SetTerrainPenaltyBias(Grid2D size, AStarPathNode[,] pathNodes)
        {
            //for (int i = 0; i < size.Row; i++)
            //{
            //    for (int j = 0; j < size.Column; j++)
            //    {
            //        if (i > 5 && j > 5 && i < 20 && j < 10)
            //        {
            //            pathNodes[i, j].TerrainWeight = -0.5f;
            //        }
            //        else if (i > 20 && j > 10 && i < 30 && j < 20)
            //        {
            //            pathNodes[i, j].TerrainWeight = 0.8f;
            //        }
            //    }
            //}
        }
    }
}