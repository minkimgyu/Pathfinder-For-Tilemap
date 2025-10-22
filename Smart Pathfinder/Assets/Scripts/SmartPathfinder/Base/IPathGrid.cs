using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfinderForTilemap
{
    public interface IPathGrid<T> where T : IPathNode<T>
    {
        void Initialize(); // �ʱ�ȭ �Լ�
        Grid2D GetPathNodeIndex(Vector2 worldPos);
        Grid2D GetGridSize(); // �׸��� ũ�� ��ȯ �Լ�
        T GetPathNode(Grid2D grid); // �׸��� ��ġ�� ��� ��ȯ �Լ�
        Vector2 GetClampedPosition(Vector2 pos); // ��ġ�� ���� ���� Ŭ�����ϴ� �Լ�
    }
}