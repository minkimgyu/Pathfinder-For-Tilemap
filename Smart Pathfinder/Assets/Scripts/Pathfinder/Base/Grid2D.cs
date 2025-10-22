using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfinderForTilemap
{
    [Serializable]
    public struct Grid2D
    {
        [SerializeField] int row;
        public int Row { get { return row; } }

        [SerializeField] int column;
        public int Column { get { return column; } }

        public Grid2D(int row, int column)
        {
            this.row = row;
            this.column = column;
        }
    }
}