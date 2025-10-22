using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;

namespace PathfinderForTilemap
{
    public class AStarPathGrid : MonoBehaviour, IPathGrid<AStarPathNode>
    {
        AStarPathNode[,] _pathNodes; // r, c

        Grid2D _gridSize;

        Vector2 _topLeftPos;
        Vector2 _bottomRightPos;

        [SerializeField] BaseAStarPathGridGenerator _pathGridGenerator;
        [SerializeField] PathNodeSO _pathNodeSO;

        public Grid2D GetGridSize() { return _gridSize; }

        AStarPathGridDrawer _drawer;

        // �ʱ�ȭ �Լ�
        public void Initialize()
        {
            if (_pathNodeSO == null) return;

            _pathNodes = _pathNodeSO.GetPathNodes();
            _gridSize = new Grid2D(_pathNodes.GetLength(0), _pathNodes.GetLength(1));

            _topLeftPos = GetPathNode(new Grid2D(0, 0)).WorldPos;
            _bottomRightPos = GetPathNode(new Grid2D(_pathNodes.GetLength(0) - 1, _pathNodes.GetLength(1) - 1)).WorldPos;

            _drawer = GetComponent<AStarPathGridDrawer>();
            if (_drawer != null) _drawer.Initialize(this);
        }

        [ContextMenu("RebuildGrid")]
        public void RG()
        {
            RebuildGrid();
        }

        public void RebuildGrid(Grid2D startIndex = default, Grid2D endIndex = default)
        {
            if (_pathGridGenerator == null)
            {
                Debug.LogError("PathGridGenerator is null!");
                return;
            }

            // grid�� (0,0) �̸� ��ü �׸��� ����
            if (endIndex.Row == 0 && endIndex.Column == 0) endIndex = _gridSize;

            AStarPathNode[,] newPathNodes = _pathGridGenerator.RebuildGrid(startIndex, endIndex);

            for (int i = startIndex.Row; i < endIndex.Row; i++)
            {
                for (int j = startIndex.Column; j < endIndex.Column; j++)
                {
                    int row = i - startIndex.Row;
                    int column = j - startIndex.Column;

                    _pathNodes[i, j].Block = newPathNodes[row, column].Block;
                    _pathNodes[i, j].NearNodeIndexes = newPathNodes[row, column].NearNodeIndexes;
                }
            }
        }

        public AStarPathNode GetPathNode(Grid2D grid) { return _pathNodes[grid.Row, grid.Column]; }

        public Vector2 GetClampedPosition(Vector2 pos)
        {
            // �ݿø��ϰ� ���� �ȿ� ������
            // �� �κ��� GridSize �ٲ�� �����ؾ���
            float xPos = Mathf.Clamp(pos.x, _topLeftPos.x, _bottomRightPos.x);
            float yPos = Mathf.Clamp(pos.y, _bottomRightPos.y, _topLeftPos.y);

            return new Vector2(xPos, yPos);
        }

        public Grid2D GetPathNodeIndex(Vector2 worldPos)
        {
            Vector2 clampedPos = GetClampedPosition(worldPos);
            Vector2 topLeftPos = GetPathNode(new Grid2D(0, 0)).WorldPos;

            int r = Mathf.RoundToInt(Mathf.Abs(topLeftPos.y - clampedPos.y));
            int c = Mathf.RoundToInt(Mathf.Abs(topLeftPos.x - clampedPos.x)); // �ε����̹Ƿ� 1 ���ش�.
            return new Grid2D(r, c);
        }
    }
}