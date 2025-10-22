using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfinderForTilemap
{
    [System.Serializable]
    public struct AStarPathNodeRow
    {
        public AStarPathNodeData[] _column;
    }

    [CreateAssetMenu(fileName = "PathNodeSO", menuName = "PathNode", order = 1)]
    public class PathNodeSO : ScriptableObject
    {
        [SerializeField] AStarPathNodeRow[] _aStarPathNodeData;
        public AStarPathNodeRow[] AStarPathNodeData { get => _aStarPathNodeData; }

        public void SetPathDatas(AStarPathNode[,] pathNodes)
        {
            AStarPathNodeRow[] aStarPathNodes;

            int rowSize = pathNodes.GetLength(0);
            int columnSize = pathNodes.GetLength(1);

            aStarPathNodes = new AStarPathNodeRow[rowSize];

            for (int i = 0; i < rowSize; i++)
            {
                aStarPathNodes[i] = new AStarPathNodeRow();
                aStarPathNodes[i]._column = new AStarPathNodeData[columnSize];

                for (int j = 0; j < columnSize; j++)
                {
                    aStarPathNodes[i]._column[j] = new AStarPathNodeData(
                        pathNodes[i, j].WorldPos,
                        pathNodes[i, j].Index,
                        pathNodes[i, j].Block,
                        pathNodes[i, j].TerrainWeight,
                        pathNodes[i, j].NearNodeIndexes[PathSize.Size1x1],
                        pathNodes[i, j].NearNodeIndexes[PathSize.Size3x3]
                    );
                }
            }

            _aStarPathNodeData = aStarPathNodes;
        }

        public AStarPathNode[,] GetPathNodes()
        {
            AStarPathNode[,] aStarPathNodes;

            int rowSize = _aStarPathNodeData.Length;
            int columnSize = _aStarPathNodeData[0]._column.Length;

            aStarPathNodes = new AStarPathNode[rowSize, columnSize];

            for (int i = 0; i < rowSize; i++)
            {
                for (int j = 0; j < columnSize; j++)
                {
                    aStarPathNodes[i, j] = new AStarPathNode(
                        _aStarPathNodeData[i]._column[j].WorldPos,
                        _aStarPathNodeData[i]._column[j].Index,
                        _aStarPathNodeData[i]._column[j].Block,
                        _aStarPathNodeData[i]._column[j].TerrainWeight,
                        new Dictionary<PathSize, List<int>>()
                        {
                            { PathSize.Size1x1, _aStarPathNodeData[i]._column[j].NearNodeIndexesSize1x1 },
                            { PathSize.Size3x3, _aStarPathNodeData[i]._column[j].NearNodeIndexesSize3x3 }
                        }
                    );
                }
            }

            return aStarPathNodes;
        }
    }
}