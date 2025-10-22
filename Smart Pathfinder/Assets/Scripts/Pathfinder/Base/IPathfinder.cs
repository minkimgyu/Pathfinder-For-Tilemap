using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfinderForTilemap
{
    public interface IPathfinder
    {
        List<Vector2> FindPath(Vector2 startPos, Vector2 targetPos, PathSize pathSize);
    }
}
