using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;

namespace AStar
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

    public class GridComponent : MonoBehaviour
    {
        [SerializeField] Tilemap _wallTile;
        [SerializeField] Tilemap _groundTile;

        Node[,] _nodes; // r, c
        Vector2 _topLeftWorldPoint;
        Vector2Int _topLeftLocalPoint;

        Grid2D _gridSize;

        List<Vector2> _points;
        const int _nodeSize = 1;

        public Node ReturnNode(Grid2D grid) { return _nodes[grid.Row, grid.Column]; }
        public Node ReturnNode(int r, int c) { return _nodes[r, c]; }

        public Vector2 ReturnClampedRange(Vector2 pos)
        {
            Vector2 topLeftPos = ReturnNode(0, 0).WorldPos;
            Vector2 bottomRightPos = ReturnNode(_nodes.GetLength(0) - 1, _nodes.GetLength(1) - 1).WorldPos;

            // �ݿø��ϰ� ���� �ȿ� ������
            // �� �κ��� GridSize �ٲ�� �����ؾ���
            float xPos = Mathf.Clamp(pos.x, topLeftPos.x, bottomRightPos.x);
            float yPos = Mathf.Clamp(pos.y, bottomRightPos.y, topLeftPos.y);

            return new Vector2(xPos, yPos);
        }

        public Grid2D ReturnNodeIndex(Vector2 worldPos)
        {
            Vector2 clampedPos = ReturnClampedRange(worldPos);
            Vector2 topLeftPos = ReturnNode(0, 0).WorldPos;

            int r = Mathf.RoundToInt(Mathf.Abs(topLeftPos.y - clampedPos.y) / _nodeSize);
            int c = Mathf.RoundToInt(Mathf.Abs(topLeftPos.x - clampedPos.x) / _nodeSize); // �ε����̹Ƿ� 1 ���ش�.
            return new Grid2D(r, c);
        }

        public List<Node> ReturnNearNodes(Grid2D index)
        {
            List<Node> nearNodes = new List<Node>();

            Grid2D[] nearIndexes = new Grid2D[]
            {
                new Grid2D(index.Row - 1, index.Column - 1), new Grid2D(index.Row - 1, index.Column), new Grid2D(index.Row - 1, index.Column + 1),

                new Grid2D(index.Row, index.Column - 1), new Grid2D(index.Row, index.Column + 1),

                new Grid2D(index.Row + 1, index.Column - 1), new Grid2D(index.Row + 1, index.Column), new Grid2D(index.Row + 1, index.Column + 1)
            };

            for (int i = 0; i < nearIndexes.Length; i++)
            {
                bool isOutOfRange =
                nearIndexes[i].Row < 0 || nearIndexes[i].Column < 0 ||
                nearIndexes[i].Row >= _gridSize.Row || nearIndexes[i].Column >= _gridSize.Column;

                if (isOutOfRange == true || ReturnNode(nearIndexes[i]).Block == true) continue;

                nearNodes.Add(ReturnNode(nearIndexes[i]));
            }

            return nearNodes;
        }

        public List<Node> ReturnNearNodes1(Grid2D index)
        {
            List<Node> nearNodes = new List<Node>();

            Grid2D[] nearIndexes = new Grid2D[]  // �� �� �� �� �� ���
                {
                    new Grid2D(index.Row - 1, index.Column),

                    new Grid2D(index.Row, index.Column - 1), new Grid2D(index.Row, index.Column + 1),

                    new Grid2D(index.Row + 1, index.Column),
                };

            for (int i = 0; i < nearIndexes.Length; i++)
            {
                bool isOutOfRange =
                nearIndexes[i].Row < 0 || nearIndexes[i].Column < 0 ||
                nearIndexes[i].Row >= _gridSize.Row || nearIndexes[i].Column >= _gridSize.Column;

                if (isOutOfRange == true || ReturnNode(nearIndexes[i]).Block == true) continue;
                nearNodes.Add(ReturnNode(nearIndexes[i]));
            }

            // �ֺ� �׸���
            Grid2D[] nearCornerIndexes = new Grid2D[]  // �밢���� ���
            {
                    new Grid2D(index.Row - 1, index.Column - 1), new Grid2D(index.Row - 1, index.Column + 1),
                    new Grid2D(index.Row + 1, index.Column - 1), new Grid2D(index.Row + 1, index.Column + 1),
            };

            // 1 _| (1, 2) |_ 2
            //(1, 3)      (2, 4)
            //  -          _
            // 3 | (3, 4) |  4

            for (int i = 0; i < nearCornerIndexes.Length; i++)
            {
                bool isOutOfRange =
                nearCornerIndexes[i].Row < 0 || nearCornerIndexes[i].Column < 0 ||
                nearCornerIndexes[i].Row >= _gridSize.Row || nearCornerIndexes[i].Column >= _gridSize.Column;

                if (isOutOfRange == true || ReturnNode(nearCornerIndexes[i]).Block == true) continue;

                // �� �� �ִ� �ڳ����� üũ
                Node node1, node2;
                switch (i)
                {
                    case 0:
                        node1 = ReturnNode(nearIndexes[0]);
                        node2 = ReturnNode(nearIndexes[1]);
                        if (node1.Block || node2.Block) continue;
                        break;
                    case 1:
                        node1 = ReturnNode(nearIndexes[0]);
                        node2 = ReturnNode(nearIndexes[2]);
                        if (node1.Block || node2.Block) continue;
                        break;
                    case 2:
                        node1 = ReturnNode(nearIndexes[1]);
                        node2 = ReturnNode(nearIndexes[3]);
                        if (node1.Block || node2.Block) continue;
                        break;
                    case 3:
                        node1 = ReturnNode(nearIndexes[2]);
                        node2 = ReturnNode(nearIndexes[3]);
                        if (node1.Block || node2.Block) continue;
                        break;
                }

                nearNodes.Add(ReturnNode(nearCornerIndexes[i]));
            }

            return nearNodes;
        }

        public List<Node> ReturnNearNodes2(Grid2D index)
        {
            List<Node> nearNodes = new List<Node>();

            Grid2D[] nearIndexes = new Grid2D[]
               {
                new Grid2D(index.Row - 1, index.Column - 1), new Grid2D(index.Row - 1, index.Column), new Grid2D(index.Row - 1, index.Column + 1),

                new Grid2D(index.Row, index.Column - 1), new Grid2D(index.Row, index.Column + 1),

                new Grid2D(index.Row + 1, index.Column - 1), new Grid2D(index.Row + 1, index.Column), new Grid2D(index.Row + 1, index.Column + 1)
               };

            for (int i = 0; i < nearIndexes.Length; i++)
            {
                bool isOutOfRange =
                nearIndexes[i].Row < 0 || nearIndexes[i].Column < 0 ||
                nearIndexes[i].Row >= _gridSize.Row || nearIndexes[i].Column >= _gridSize.Column;

                if (isOutOfRange == true || HaveBlockNodeInNearPosition(nearIndexes[i]) == true || ReturnNode(index).Block == true) continue;
                nearNodes.Add(ReturnNode(nearIndexes[i]));
            }

            return nearNodes;
        }

        public bool HaveBlockNodeInNearPosition(Grid2D index)
        {
            List<Node> nearNodes = new List<Node>();
            Grid2D[] nearIndexes = new Grid2D[]  // �� �� �� �� �� ���
            {
                new Grid2D(index.Row - 1, index.Column - 1), new Grid2D(index.Row - 1, index.Column), new Grid2D(index.Row - 1, index.Column + 1),

                new Grid2D(index.Row, index.Column - 1), new Grid2D(index.Row, index.Column + 1),

                new Grid2D(index.Row + 1, index.Column - 1), new Grid2D(index.Row + 1, index.Column), new Grid2D(index.Row + 1, index.Column + 1)
            };

            for (int i = 0; i < nearIndexes.Length; i++)
            {
                bool isOutOfRange =
                   nearIndexes[i].Row < 0 || nearIndexes[i].Column < 0 ||
                   nearIndexes[i].Row >= _gridSize.Row || nearIndexes[i].Column >= _gridSize.Column;

                if (isOutOfRange == false && ReturnNode(nearIndexes[i]).Block == true) return true;
            }

            return false;
        }

        void CreateNode()
        {
            for (int i = 0; i < _gridSize.Row; i++)
            {
                for (int j = 0; j < _gridSize.Column; j++)
                {
                    Vector2Int localPos = _topLeftLocalPoint + new Vector2Int(j, -i);
                    Vector2 worldPos = _topLeftWorldPoint + new Vector2Int(j, -i);

                    TileBase tile = _wallTile.GetTile(new Vector3Int(localPos.x, localPos.y, 0));
                    if (tile == null)
                    {
                        _nodes[i, j] = new Node(worldPos, new Grid2D(i, j), false);
                    }
                    else
                    {
                        _nodes[i, j] = new Node(worldPos, new Grid2D(i, j), true);
                    }
                    // Ÿ���� ���ٸ� �ٴ�
                    // Ÿ���� �����Ѵٸ� ��
                }
            }

            for (int i = 0; i < _gridSize.Row; i++)
            {
                for (int j = 0; j < _gridSize.Column; j++)
                {
                    _nodes[i, j].NearNodes = ReturnNearNodes(new Grid2D(i, j));
                    //_nodes[i, j].NearNodes = ReturnNearNodes1(new Grid2D(i, j));
                    //_nodes[i, j].NearNodes = ReturnNearNodes2(new Grid2D(i, j));
                }
            }

            Debug.Log("CreateNode");
        }

        private void OnDrawGizmos()
        {
            if (_points == null) return;

            for (int i = 1; i < _points.Count; i++)
            {
                Gizmos.color = new Color(0, 1, 1, 0.1f);
                Gizmos.DrawLine(_points[i - 1], _points[i]);
            }

            if (_nodes == null) return;

            for (int i = 0; i < _nodes.GetLength(0); i++)
            {
                for (int j = 0; j < _nodes.GetLength(1); j++)
                {
                    if (_nodes[i, j].Block)
                    {
                        Gizmos.color = new Color(1, 0, 0, 0.1f);
                        Gizmos.DrawCube(_nodes[i, j].WorldPos, Vector3.one);
                    }
                    else
                    {
                        Gizmos.color = new Color(0, 0, 1, 0.1f);
                        Gizmos.DrawCube(_nodes[i, j].WorldPos, Vector3.one);
                    }
                }
            }
        }

        public void Initialize(AStarNoDelay pathfinder)
        {
            _groundTile.CompressBounds(); // Ÿ���� �ٿ������ �����ش�.
            _wallTile.CompressBounds(); // Ÿ���� �ٿ������ �����ش�.
            BoundsInt bounds = _groundTile.cellBounds;

            int rowSize = bounds.yMax - bounds.yMin;
            int columnSize = bounds.xMax - bounds.xMin;

            _topLeftLocalPoint = new Vector2Int(bounds.xMin, bounds.yMax - 1);
            _topLeftWorldPoint = new Vector2(transform.position.x + bounds.xMin + _groundTile.tileAnchor.x, transform.position.y + bounds.yMax - _groundTile.tileAnchor.y);

            Debug.Log(_topLeftLocalPoint);
            Debug.Log(_topLeftWorldPoint);

            _gridSize = new Grid2D(rowSize, columnSize);
            _points = new List<Vector2>();
            _nodes = new Node[_gridSize.Row, _gridSize.Column];
            CreateNode();

            pathfinder.Initialize(this);
        }

        public void Initialize(AStar pathfinder)
        {
            _groundTile.CompressBounds(); // Ÿ���� �ٿ������ �����ش�.
            _wallTile.CompressBounds(); // Ÿ���� �ٿ������ �����ش�.
            BoundsInt bounds = _groundTile.cellBounds;

            int rowSize = bounds.yMax - bounds.yMin;
            int columnSize = bounds.xMax - bounds.xMin;

            _topLeftLocalPoint = new Vector2Int(bounds.xMin, bounds.yMax - 1);
            _topLeftWorldPoint = new Vector2(transform.position.x + bounds.xMin + _groundTile.tileAnchor.x, transform.position.y + bounds.yMax - _groundTile.tileAnchor.y);

            Debug.Log(_topLeftLocalPoint);
            Debug.Log(_topLeftWorldPoint);

            _gridSize = new Grid2D(rowSize, columnSize);
            _points = new List<Vector2>();
            _nodes = new Node[_gridSize.Row, _gridSize.Column];
            CreateNode();

            pathfinder.Initialize(this);
        }
    }
}