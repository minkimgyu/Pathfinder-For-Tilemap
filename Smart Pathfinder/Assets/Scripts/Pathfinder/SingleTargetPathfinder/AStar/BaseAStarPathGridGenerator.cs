using PathfinderForTilemap;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PathfinderForTilemap
{
    abstract public class BaseAStarPathGridGenerator : MonoBehaviour, IPathGridGenerator
    {
        [SerializeField] Tilemap _wallTile;
        [SerializeField] Tilemap _groundTile;

        //protected AStarPathNode[,] _pathNodes; // r, c
        //protected Grid2D _gridSize;

        Vector2 _topLeftWorldPoint;
        Vector2Int _topLeftLocalPoint;

        [SerializeField] PathNodeSO _pathNodeSO;

        /// <summary>
        /// �׸��� �� ����
        /// </summary>
        [ContextMenu("GenerateGrid")]
        public void GenerateGrid()
        {
#if UNITY_EDITOR
            _groundTile.CompressBounds(); // Ÿ���� �ٿ������ �����ش�.
            _wallTile.CompressBounds(); // Ÿ���� �ٿ������ �����ش�.
            BoundsInt bounds = _groundTile.cellBounds;

            int rowSize = bounds.yMax - bounds.yMin;
            int columnSize = bounds.xMax - bounds.xMin;

            _topLeftLocalPoint = new Vector2Int(bounds.xMin, bounds.yMax - 1);
            _topLeftWorldPoint = new Vector2(transform.position.x + bounds.xMin + _groundTile.tileAnchor.x, transform.position.y + bounds.yMax - _groundTile.tileAnchor.y);

            Grid2D gridSize = new Grid2D(rowSize, columnSize);
            AStarPathNode[,] pathNodes = new AStarPathNode[gridSize.Row, gridSize.Column];

            pathNodes = CreateNode(new Grid2D(0, 0), new Grid2D(rowSize, columnSize));
            SetTerrainPenaltyBias(gridSize, pathNodes);

            if (_pathNodeSO == null) return;
            _pathNodeSO.SetPathDatas(pathNodes);
            EditorUtility.SetDirty(_pathNodeSO);
            AssetDatabase.SaveAssets();
#endif
        }

        /// <summary>
        /// �׸��� �籸��
        /// ���� ����(block ����)�� ����Ǹ� ȣ��
        /// </summary>
        public AStarPathNode[,] RebuildGrid(Grid2D startIndex, Grid2D endIndex)
        {
            BoundsInt bounds = _groundTile.cellBounds;

            _topLeftLocalPoint = new Vector2Int(bounds.xMin, bounds.yMax - 1);
            _topLeftWorldPoint = new Vector2(transform.position.x + bounds.xMin + _groundTile.tileAnchor.x, transform.position.y + bounds.yMax - _groundTile.tileAnchor.y);

            Grid2D gridSize = new Grid2D(endIndex.Row - startIndex.Row, endIndex.Column - startIndex.Column);
            AStarPathNode[,] pathNodes = new AStarPathNode[gridSize.Row, gridSize.Column];
            pathNodes = CreateNode(startIndex, endIndex);
            return pathNodes;
        }

        bool OutOfRange(Grid2D size, Grid2D index)
        {
            return index.Row < 0 || index.Column < 0 || index.Row >= size.Row || index.Column >= size.Column;
        }

        // 1x1 ��ü �̵� ���� ��� ��ȯ
        List<int> GetNearMovableNodeIndexesSize1x1(AStarPathNode[,] pathNodes, Grid2D size, Grid2D index)
        {
            List<int> nearNodes = new List<int>();

            for (int i = 0; i < GridUtility.NearStraightIndexes.Length; i++)
            {
                Grid2D straightIndex = GridUtility.NearIndexes[GridUtility.NearStraightIndexes[i]];
                Grid2D nearIndex = new Grid2D(index.Row + straightIndex.Row, index.Column + straightIndex.Column);

                if (OutOfRange(size, nearIndex) == true || pathNodes[nearIndex.Row, nearIndex.Column].Block == true) continue;
                nearNodes.Add(GridUtility.NearStraightIndexes[i]);
            }

            // 1 _| (1, 2) |_ 2
            //(1, 3)      (2, 4)
            //  -          _
            // 3 | (3, 4) |  4

            for (int i = 0; i < GridUtility.NearDiagonalIndexes.Length; i++)
            {
                Grid2D diagonalIndex = GridUtility.NearIndexes[GridUtility.NearDiagonalIndexes[i]];
                Grid2D nearIndex = new Grid2D(index.Row + diagonalIndex.Row, index.Column + diagonalIndex.Column);

                if (OutOfRange(size, nearIndex) == true || pathNodes[nearIndex.Row, nearIndex.Column].Block == true) continue;

                // �� �� �ִ� �ڳ����� üũ
                AStarPathNode node1, node2;
                Grid2D grid1, grid2;

                switch (i)
                {
                    case 0:
                        grid1 = new Grid2D(index.Row + GridUtility.NearIndexes[GridUtility.NearStraightIndexes[0]].Row, index.Column + GridUtility.NearIndexes[GridUtility.NearStraightIndexes[0]].Column);
                        grid2 = new Grid2D(index.Row + GridUtility.NearIndexes[GridUtility.NearStraightIndexes[1]].Row, index.Column + GridUtility.NearIndexes[GridUtility.NearStraightIndexes[1]].Column);
                        if (OutOfRange(size, grid1) || OutOfRange(size, grid2)) continue;

                        node1 = pathNodes[grid1.Row, grid1.Column];
                        node2 = pathNodes[grid2.Row, grid2.Column];
                        if (node1.Block || node2.Block) continue;
                        break;
                    case 1:
                        grid1 = new Grid2D(index.Row + GridUtility.NearIndexes[GridUtility.NearStraightIndexes[0]].Row, index.Column + GridUtility.NearIndexes[GridUtility.NearStraightIndexes[0]].Column);
                        grid2 = new Grid2D(index.Row + GridUtility.NearIndexes[GridUtility.NearStraightIndexes[2]].Row, index.Column + GridUtility.NearIndexes[GridUtility.NearStraightIndexes[2]].Column);
                        if (OutOfRange(size, grid1) || OutOfRange(size, grid2)) continue;

                        node1 = pathNodes[grid1.Row, grid1.Column];
                        node2 = pathNodes[grid2.Row, grid2.Column];
                        if (node1.Block || node2.Block) continue;
                        break;
                    case 2:
                        grid1 = new Grid2D(index.Row + GridUtility.NearIndexes[GridUtility.NearStraightIndexes[1]].Row, index.Column + GridUtility.NearIndexes[GridUtility.NearStraightIndexes[1]].Column);
                        grid2 = new Grid2D(index.Row + GridUtility.NearIndexes[GridUtility.NearStraightIndexes[3]].Row, index.Column + GridUtility.NearIndexes[GridUtility.NearStraightIndexes[3]].Column);
                        if (OutOfRange(size, grid1) || OutOfRange(size, grid2)) continue;

                        node1 = pathNodes[grid1.Row, grid1.Column];
                        node2 = pathNodes[grid2.Row, grid2.Column];
                        if (node1.Block || node2.Block) continue;
                        break;
                    case 3:
                        grid1 = new Grid2D(index.Row + GridUtility.NearIndexes[GridUtility.NearStraightIndexes[2]].Row, index.Column + GridUtility.NearIndexes[GridUtility.NearStraightIndexes[2]].Column);
                        grid2 = new Grid2D(index.Row + GridUtility.NearIndexes[GridUtility.NearStraightIndexes[3]].Row, index.Column + GridUtility.NearIndexes[GridUtility.NearStraightIndexes[3]].Column);
                        if (OutOfRange(size, grid1) || OutOfRange(size, grid2)) continue;

                        node1 = pathNodes[grid1.Row, grid1.Column];
                        node2 = pathNodes[grid2.Row, grid2.Column];
                        if (node1.Block || node2.Block) continue;
                        break;
                }

                nearNodes.Add(GridUtility.NearDiagonalIndexes[i]);
            }

            return nearNodes;
        }

        // 3x3 ��ü �̵� ���� ��� ��ȯ
        List<int> GetNearMovableNodeIndexesSize3x3(AStarPathNode[,] pathNodes, Grid2D size, Grid2D index)
        {
            List<int> nearNodes = new List<int>();

            for (int i = 0; i < GridUtility.NearDiagonalIndexes.Length; i++)
            {
                Grid2D nearIndex = new Grid2D(index.Row + GridUtility.NearIndexes[GridUtility.NearDiagonalIndexes[i]].Row, index.Column + GridUtility.NearIndexes[GridUtility.NearDiagonalIndexes[i]].Column);
                if (OutOfRange(size, nearIndex) == true || HaveBlockNodeInNearPosition(pathNodes, size, nearIndex) == true || pathNodes[nearIndex.Row, nearIndex.Column].Block == true) continue;
                nearNodes.Add(GridUtility.NearDiagonalIndexes[i]);
            }

            for (int i = 0; i < GridUtility.NearStraightIndexes.Length; i++)
            {
                Grid2D nearIndex = new Grid2D(index.Row + GridUtility.NearIndexes[GridUtility.NearStraightIndexes[i]].Row, index.Column + GridUtility.NearIndexes[GridUtility.NearStraightIndexes[i]].Column);
                if (OutOfRange(size, nearIndex) == true || HaveBlockNodeInNearPosition(pathNodes, size, nearIndex) == true || pathNodes[nearIndex.Row, nearIndex.Column].Block == true) continue;
                nearNodes.Add(GridUtility.NearStraightIndexes[i]);
            }

            return nearNodes;
        }

        // �ش� ��ġ �ֺ��� Block ��尡 �ִ��� Ȯ��
        bool HaveBlockNodeInNearPosition(AStarPathNode[,] pathNodes, Grid2D size, Grid2D index)
        {
            List<AStarPathNode> nearNodes = new List<AStarPathNode>();

            for (int i = 0; i < GridUtility.NearStraightIndexes.Length; i++)
            {
                Grid2D nearIndex = new Grid2D(index.Row + GridUtility.NearIndexes[GridUtility.NearStraightIndexes[i]].Row, index.Column + GridUtility.NearIndexes[GridUtility.NearStraightIndexes[i]].Column);
                if (OutOfRange(size, nearIndex) == false && pathNodes[nearIndex.Row, nearIndex.Column].Block == true) return true;
            }

            for (int i = 0; i < GridUtility.NearDiagonalIndexes.Length; i++)
            {
                Grid2D nearIndex = new Grid2D(index.Row + GridUtility.NearIndexes[GridUtility.NearDiagonalIndexes[i]].Row, index.Column + GridUtility.NearIndexes[GridUtility.NearDiagonalIndexes[i]].Column);
                if (OutOfRange(size, nearIndex) == false && pathNodes[nearIndex.Row, nearIndex.Column].Block == true) return true;
            }

            return false;
        }

        public List<int> ReturnNearNodes(AStarPathNode[,] pathNodes, Grid2D gridSize, Grid2D index, PathSize pathSize)
        {
            switch (pathSize)
            {
                case PathSize.Size1x1:
                    return GetNearMovableNodeIndexesSize1x1(pathNodes, gridSize, index);
                case PathSize.Size3x3:
                    return GetNearMovableNodeIndexesSize3x3(pathNodes, gridSize, index);
                default:
                    return null;
            }
        }

        // ��� ����
        AStarPathNode[,] CreateNode(Grid2D startIndex, Grid2D endIndex)
        {
            int rowSize = endIndex.Row - startIndex.Row;
            int columnSize = endIndex.Column - startIndex.Column;
            Grid2D gridSize = new Grid2D(rowSize, columnSize);

            AStarPathNode[,] pathNodes = new AStarPathNode[rowSize, columnSize];

            for (int i = startIndex.Row; i < endIndex.Row; i++)
            {
                for (int j = startIndex.Column; j < endIndex.Column; j++)
                {
                    Vector2Int localPos = _topLeftLocalPoint + new Vector2Int(j, -i);
                    Vector2 worldPos = _topLeftWorldPoint + new Vector2Int(j, -i);

                    TileBase tile = _wallTile.GetTile(new Vector3Int(localPos.x, localPos.y, 0));
                    bool isBlock = tile != null;
                    // Ÿ���� ���ٸ� �ٴ�
                    // Ÿ���� �����Ѵٸ� ��

                    int row = i - startIndex.Row;
                    int column = j - startIndex.Column;
                    pathNodes[row, column] = new AStarPathNode(worldPos, new Grid2D(i, j), isBlock);
                }
            }

            for (int i = startIndex.Row; i < rowSize; i++)
            {
                for (int j = startIndex.Column; j < columnSize; j++)
                {
                    int row = i - startIndex.Row;
                    int column = j - startIndex.Column;

                    pathNodes[row, column].NearNodeIndexes[PathSize.Size1x1] = ReturnNearNodes(pathNodes, gridSize, new Grid2D(i, j), PathSize.Size1x1);
                    pathNodes[row, column].NearNodeIndexes[PathSize.Size3x3] = ReturnNearNodes(pathNodes, gridSize, new Grid2D(i, j), PathSize.Size3x3);
                }
            }

            return pathNodes;
        }

        // ����ġ ����
        abstract protected void SetTerrainPenaltyBias(Grid2D size, AStarPathNode[,] pathNodes);
    }
}