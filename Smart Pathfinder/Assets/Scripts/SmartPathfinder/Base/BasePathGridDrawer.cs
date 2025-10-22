using PathfinderForTilemap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfinderForTilemap
{
    abstract public class BaseDrawer : MonoBehaviour
    {
        protected abstract void DrawGrid();

        private void OnDrawGizmos()
        {
            DrawGrid();
        }
    }
}