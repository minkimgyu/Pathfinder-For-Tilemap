using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfinderForTilemap
{
    public interface IPathGrid<T> where T : IPathNode<T>
    {
        void Initialize(); // 초기화 함수
        Grid2D GetPathNodeIndex(Vector2 worldPos);
        Grid2D GetGridSize(); // 그리드 크기 반환 함수
        T GetPathNode(Grid2D grid); // 그리드 위치로 노드 반환 함수
        Vector2 GetClampedPosition(Vector2 pos); // 위치를 범위 내로 클램핑하는 함수
    }
}