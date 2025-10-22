using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;

namespace JPSPlus
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

        public static int Diff(Grid2D a, Grid2D b)
        {
            // because diagonal
            // 0,0 diff 1,1 = 1 
            // 0,0 diff 0,1 = 1 
            // 0,0 diff 1,2 = 2 
            // 0,0 diff 2,2 = 2 
            // return max of the diff row or diff column
            int diff_columns = Mathf.Abs(b.column - a.column);
            int diff_rows = Mathf.Abs(b.row - a.row);

            return Mathf.Max(diff_rows, diff_columns);
        }
    }

    public class GridComponent : MonoBehaviour
    {
        [SerializeField] Tilemap _wallTile;
        [SerializeField] Tilemap _groundTile;

        Node[,] _nodes; // r, c
        Vector2 _topLeftWorldPoint;
        Vector2Int _topLeftLocalPoint;

        [SerializeField] Size _size = Size.x1;

        Grid2D _gridSize;

        List<Vector2> _points;
        const int _nodeSize = 1;

        public Node ReturnNode(Grid2D grid) { return _nodes[grid.Row, grid.Column]; }
        public Node ReturnNode(int r, int c) { return _nodes[r, c]; }

        Vector2Int _bottomIndex;

        public Vector2 ReturnClampedRange(Vector2 pos)
        {
            Vector2 topLeftPos = ReturnNode(0, 0).WorldPos;
            Vector2 bottomRightPos = ReturnNode(_gridSize.Row - 1, _gridSize.Column - 1).WorldPos;

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

        Grid2D[] _direction = 
        {
            new Grid2D(-1, 0), // NORTH
            new Grid2D(-1, 1), // NORTH_EAST
            new Grid2D(0, 1), // EAST
            new Grid2D(1, 1), // SOUTH_EAST
            new Grid2D(1, 0), // SOUTH
            new Grid2D(1, -1), // SOUTH_WEST
            new Grid2D(0, -1), // WEST
            new Grid2D(-1, -1), // NORTH_WEST
        };

        public Tuple<bool[], Node[], float[], bool> GetNeighborInfo(Grid2D index)
        {
            int directionSize = _direction.Length;

            bool[] haveNodes = new bool[directionSize];
            Node[] nearNodes = new Node[directionSize];
            float[] nearNodesDistance = new float[directionSize];

            bool haveNearBlockNode = false;

            Node currentNode = ReturnNode(new Grid2D(index.Row, index.Column));

            for (int i = 0; i < directionSize; i++)
            {
                Grid2D newGrid = new Grid2D(index.Row + _direction[i].Row, index.Column + _direction[i].Column);
                bool isOutOfRange = IsOutOfRange(newGrid);
                if (isOutOfRange == true)
                {
                    haveNodes[i] = false;
                    continue;
                }

                nearNodes[i] = ReturnNode(newGrid);
                if (nearNodes[i].IsBlock(Size.x1) == true)
                {
                    haveNearBlockNode = true;
                }
                nearNodesDistance[i] = Vector2.Distance(nearNodes[i].WorldPos, currentNode.WorldPos);
                haveNodes[i] = true;
            }

            return new Tuple<bool[], Node[], float[], bool>(haveNodes, nearNodes, nearNodesDistance, haveNearBlockNode);
        }


        public bool IsOutOfRange(Grid2D index) 
        {
            return index.Row < 0 || index.Column < 0 || index.Row >= _gridSize.Row || index.Column >= _gridSize.Column;
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
                    Tuple<bool[], Node[], float[], bool> nodeDatas = GetNeighborInfo(new Grid2D(i, j));
                    _nodes[i, j].HaveNodes = nodeDatas.Item1;
                    _nodes[i, j].NearNodes = nodeDatas.Item2;
                    _nodes[i, j].NearNodeDistances = nodeDatas.Item3;
                    _nodes[i, j].HaveNearBlockNode = nodeDatas.Item4;
                }
            }

            Debug.Log("CreateNode");
        }

        [SerializeField] bool _showPrimaryJumpDirection;

        [SerializeField] bool _showPrimaryJumpPoint;

        [SerializeField] bool _showJumpPointDistances;

        private void OnDrawGizmos()
        {
            if (_points == null) return;

            for (int i = 1; i < _points.Count; i++)
            {
                Gizmos.color = new Color(0, 1, 1, 0.1f);
                Gizmos.DrawLine(_points[i - 1], _points[i]);
            }

            if (_nodes == null) return;

            for (int i = 0; i < _gridSize.Row; i++)
            {
                for (int j = 0; j < _gridSize.Column; j++)
                {
                    if (_nodes[i, j].IsBlock(_size))
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

            if(_showPrimaryJumpPoint)
            {
                for (int i = 0; i < _primaryJumpPoints.Count; i++)
                {
                    Gizmos.color = new Color(0, 1, 1, 1f);
                    Gizmos.DrawCube(_primaryJumpPoints[i], Vector3.one * 0.3f);
                }
            }

            if (_showPrimaryJumpDirection)
            {
                for (int i = 0; i < _gridSize.Row; i++)
                {
                    for (int j = 0; j < _gridSize.Column; j++)
                    {
                        if (_nodes[i, j].IsJumpPoint)
                        {
                            for (int k = 0; k < 9; k++)
                            {
                                switch (k)
                                {
                                    case 0:
                                        if (_nodes[i, j].JumpPointDirections[k] == true)
                                        {
                                            Handles.Label(_nodes[i, j].WorldPos + new Vector2(0, 0.3f), "��", _emptyStyle);
                                        }
                                        break;
                                    case 1:
                                        if (_nodes[i, j].JumpPointDirections[k] == true)
                                        {
                                            Handles.Label(_nodes[i, j].WorldPos + new Vector2(0.3f, 0.3f), "��", _emptyStyle);
                                        }
                                        break;
                                    case 2:
                                        if (_nodes[i, j].JumpPointDirections[k] == true)
                                        {
                                            Handles.Label(_nodes[i, j].WorldPos + new Vector2(0.3f, 0), "��", _emptyStyle);
                                        }
                                        break;
                                    case 3:
                                        if (_nodes[i, j].JumpPointDirections[k] == true)
                                        {
                                            Handles.Label(_nodes[i, j].WorldPos + new Vector2(0.3f, -0.3f), "��", _emptyStyle);
                                        }
                                        break;
                                    case 4:
                                        if (_nodes[i, j].JumpPointDirections[k] == true)
                                        {
                                            Handles.Label(_nodes[i, j].WorldPos + new Vector2(0, -0.3f), "��", _emptyStyle);
                                        }
                                        break;
                                    case 5:
                                        if (_nodes[i, j].JumpPointDirections[k] == true)
                                        {
                                            Handles.Label(_nodes[i, j].WorldPos + new Vector2(-0.3f, -0.3f), "��", _emptyStyle);
                                        }
                                        break;
                                    case 6:
                                        if (_nodes[i, j].JumpPointDirections[k] == true)
                                        {
                                            Handles.Label(_nodes[i, j].WorldPos + new Vector2(-0.3f, 0), "��", _emptyStyle);
                                        }
                                        break;
                                    case 7:
                                        if (_nodes[i, j].JumpPointDirections[k] == true)
                                        {
                                            Handles.Label(_nodes[i, j].WorldPos + new Vector2(-0.3f, 0.3f), "��", _emptyStyle);
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            if (_showJumpPointDistances)
            {
                for (int i = 0; i < _gridSize.Row; i++)
                {
                    for (int j = 0; j < _gridSize.Column; j++)
                    {
                        if (_nodes[i, j].IsBlock(_size) == true) continue;

                        for (int k = 0; k < 9; k++)
                        {
                            switch (k)
                            {
                                case 0:
                                    Handles.Label(_nodes[i, j].WorldPos + new Vector2(0, 0.3f), _nodes[i, j].JumpPointDistances[k].ToString(), _emptyStyle);
                                    break;
                                case 1:
                                    Handles.Label(_nodes[i, j].WorldPos + new Vector2(0.3f, 0.3f), _nodes[i, j].JumpPointDistances[k].ToString(), _emptyStyle);
                                    break;
                                case 2:
                                    Handles.Label(_nodes[i, j].WorldPos + new Vector2(0.3f, 0), _nodes[i, j].JumpPointDistances[k].ToString(), _emptyStyle);
                                    break;
                                case 3:
                                    Handles.Label(_nodes[i, j].WorldPos + new Vector2(0.3f, -0.3f), _nodes[i, j].JumpPointDistances[k].ToString(), _emptyStyle);
                                    break;
                                case 4:
                                    Handles.Label(_nodes[i, j].WorldPos + new Vector2(0, -0.3f), _nodes[i, j].JumpPointDistances[k].ToString(), _emptyStyle);
                                    break;
                                case 5:
                                    Handles.Label(_nodes[i, j].WorldPos + new Vector2(-0.3f, -0.3f), _nodes[i, j].JumpPointDistances[k].ToString(), _emptyStyle);
                                    break;
                                case 6:
                                    Handles.Label(_nodes[i, j].WorldPos + new Vector2(-0.3f, 0), _nodes[i, j].JumpPointDistances[k].ToString(), _emptyStyle);
                                    break;
                                case 7:
                                    Handles.Label(_nodes[i, j].WorldPos + new Vector2(-0.3f, 0.3f), _nodes[i, j].JumpPointDistances[k].ToString(), _emptyStyle);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        GUIStyle _emptyStyle = new GUIStyle();

        public void Initialize(FastJPSPlus pathfinder)
        {
            _emptyStyle.fontSize = 20;
            _emptyStyle.alignment = TextAnchor.MiddleCenter;
            _emptyStyle.normal.textColor = Color.red;

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
            BuildPrimaryJumpPoints();
            BuildStraightJumpPoints();
            BuildDiagonalJumpPoints();
        }

        List<Vector2> _primaryJumpPoints = new List<Vector2>();


        public Node GetNodeDist(int row, int column, int direction, int dist)
        {
            Node new_node = null;
            int new_row = row, new_column = column;

            switch (direction)
            {
                case 0:
                    new_row -= dist;
                    break;
                case 1:
                    new_row -= dist;
                    new_column += dist;
                    break;
                case 2:
                    new_column += dist;
                    break;
                case 3:
                    new_row += dist;
                    new_column += dist;
                    break;
                case 4:
                    new_row += dist;
                    break;
                case 5:
                    new_row += dist;
                    new_column -= dist;
                    break;
                case 6:
                    new_column -= dist;
                    break;
                case 7:
                    new_row -= dist;
                    new_column -= dist;
                    break;
            }

            // w/ the new coordinates, get the node
            if (IsOutOfRange(new Grid2D(new_row, new_column)) == false)
            {
                new_node = _nodes[new_row, new_column];
            }

            return new_node;
        }



        public void BuildPrimaryJumpPoints()
        {
            for (int i = 0; i < _gridSize.Row; i++)
            {
                for (int j = 0; j < _gridSize.Column; j++)
                {
                    Node currentNode = _nodes[i, j];
                    if (currentNode.IsBlock(_size) == false) continue; // ���� Block�� �ƴϸ� continue

                    Grid2D northEastIndex = new Grid2D(currentNode.Index.Row + _direction[1].Row, currentNode.Index.Column + _direction[1].Column);

                    if(IsOutOfRange(northEastIndex) == false && _nodes[northEastIndex.Row, northEastIndex.Column].IsBlock(_size) == false)
                    {
                        Node node = _nodes[northEastIndex.Row, northEastIndex.Column];
                        
                        Grid2D southIndex = new Grid2D(node.Index.Row + _direction[4].Row, node.Index.Column + _direction[4].Column);
                        Grid2D westIndex = new Grid2D(node.Index.Row + _direction[6].Row, node.Index.Column + _direction[6].Column);

                        if(IsOutOfRange(southIndex) == false && _nodes[southIndex.Row, southIndex.Column].IsBlock(_size) == false &&
                            IsOutOfRange(westIndex) == false && _nodes[westIndex.Row, westIndex.Column].IsBlock(_size) == false)
                        {
                            node.IsJumpPoint = true;
                            node.JumpPointDirections[4] = true;
                            node.JumpPointDirections[6] = true;
                            _primaryJumpPoints.Add(node.WorldPos);
                        }
                    }

                    Grid2D southEastIndex = new Grid2D(currentNode.Index.Row + _direction[3].Row, currentNode.Index.Column + _direction[3].Column);

                    if (IsOutOfRange(southEastIndex) == false && _nodes[southEastIndex.Row, southEastIndex.Column].IsBlock(_size) == false)
                    {
                        Node node = _nodes[southEastIndex.Row, southEastIndex.Column];

                        Grid2D northIndex = new Grid2D(node.Index.Row + _direction[0].Row, node.Index.Column + _direction[0].Column);
                        Grid2D westIndex = new Grid2D(node.Index.Row + _direction[6].Row, node.Index.Column + _direction[6].Column);

                        if (IsOutOfRange(northIndex) == false && _nodes[northIndex.Row, northIndex.Column].IsBlock(_size) == false &&
                            IsOutOfRange(westIndex) == false && _nodes[westIndex.Row, westIndex.Column].IsBlock(_size) == false)
                        {
                            node.IsJumpPoint = true;
                            node.JumpPointDirections[0] = true;
                            node.JumpPointDirections[6] = true;
                            _primaryJumpPoints.Add(node.WorldPos);
                        }
                    }

                    Grid2D southWestIndex = new Grid2D(currentNode.Index.Row + _direction[5].Row, currentNode.Index.Column + _direction[5].Column);

                    if (IsOutOfRange(southWestIndex) == false && _nodes[southWestIndex.Row, southWestIndex.Column].IsBlock(_size) == false)
                    {
                        Node node = _nodes[southWestIndex.Row, southWestIndex.Column];

                        Grid2D northIndex = new Grid2D(node.Index.Row + _direction[0].Row, node.Index.Column + _direction[0].Column);
                        Grid2D eastIndex = new Grid2D(node.Index.Row + _direction[2].Row, node.Index.Column + _direction[2].Column);

                        if (IsOutOfRange(northIndex) == false && _nodes[northIndex.Row, northIndex.Column].IsBlock(_size) == false &&
                            IsOutOfRange(eastIndex) == false && _nodes[eastIndex.Row, eastIndex.Column].IsBlock(_size) == false)
                        {
                            node.IsJumpPoint = true;
                            node.JumpPointDirections[0] = true;
                            node.JumpPointDirections[2] = true;
                            _primaryJumpPoints.Add(node.WorldPos);
                        }
                    }

                    Grid2D northWestIndex = new Grid2D(currentNode.Index.Row + _direction[7].Row, currentNode.Index.Column + _direction[7].Column);

                    if (IsOutOfRange(northWestIndex) == false && _nodes[northWestIndex.Row, northWestIndex.Column].IsBlock(_size) == false)
                    {
                        Node node = _nodes[northWestIndex.Row, northWestIndex.Column];

                        Grid2D southIndex = new Grid2D(node.Index.Row + _direction[4].Row, node.Index.Column + _direction[4].Column);
                        Grid2D eastIndex = new Grid2D(node.Index.Row + _direction[2].Row, node.Index.Column + _direction[2].Column);

                        if (IsOutOfRange(southIndex) == false && _nodes[southIndex.Row, southIndex.Column].IsBlock(_size) == false &&
                            IsOutOfRange(eastIndex) == false && _nodes[eastIndex.Row, eastIndex.Column].IsBlock(_size) == false)
                        {
                            node.IsJumpPoint = true;
                            node.JumpPointDirections[4] = true;
                            node.JumpPointDirections[2] = true;
                            _primaryJumpPoints.Add(node.WorldPos);
                        }
                    }
                }
            }
        }
        public void BuildStraightJumpPoints()
        {
            for (int i = 0; i < _gridSize.Row; i++)
            {
                // Calc moving left to right
                int jumpDistanceSoFar = -1;
                bool jumpPointSeen = false;

                for (int j = 0; j < _gridSize.Column; j++)
                {
                    Node node = _nodes[i, j];

                    // If we've reach a wall, then reset everything :(
                    if (node.IsBlock(_size))
                    {
                        jumpDistanceSoFar = -1;
                        jumpPointSeen = false;
                        node.JumpPointDistances[6] = 0; // west
                        continue;
                    }

                    jumpDistanceSoFar++;

                    if (jumpPointSeen)
                    {
                        // If we've seen a jump point heading left, then we can tell this node he's got a jump point coming up to his LEFT ( WEST )
                        node.JumpPointDistances[6] = jumpDistanceSoFar;
                    }
                    else
                    {
                        node.JumpPointDistances[6] = -jumpDistanceSoFar;   // Set wall distance
                    }

                    // If we just found a new jump point, then set everything up for this new jump point
                    if (node.JumpPointDirections[2])
                    {
                        jumpDistanceSoFar = 0;
                        jumpPointSeen = true;
                    }
                }

                jumpDistanceSoFar = -1;
                jumpPointSeen = false;
                for (int j = _gridSize.Column - 1; j > -1; j--)
                {
                    Node node = _nodes[i, j];

                    // If we've reach a wall, then reset everything :(
                    if (node.IsBlock(_size))
                    {
                        jumpDistanceSoFar = -1;
                        jumpPointSeen = false;
                        node.JumpPointDistances[2] = 0; // east
                        continue;
                    }

                    jumpDistanceSoFar++;

                    if (jumpPointSeen)
                    {
                        // If we've seen a jump point heading left, then we can tell this node he's got a jump point coming up to his LEFT ( WEST )
                        node.JumpPointDistances[2] = jumpDistanceSoFar;
                    }
                    else
                    {
                        node.JumpPointDistances[2] = -jumpDistanceSoFar;   // Set wall distance
                    }

                    // If we just found a new jump point, then set everything up for this new jump point
                    if (node.JumpPointDirections[6])
                    {
                        jumpDistanceSoFar = 0;
                        jumpPointSeen = true;
                    }
                }
            }




            for (int i = 0; i < _gridSize.Column; i++)
            {
                // Calc moving left to right
                int jumpDistanceSoFar = -1;
                bool jumpPointSeen = false;

                for (int j = 0; j < _gridSize.Row; j++)
                {
                    Node node = _nodes[j, i];

                    // If we've reach a wall, then reset everything :(
                    if (node.IsBlock(_size))
                    {
                        jumpDistanceSoFar = -1;
                        jumpPointSeen = false;
                        node.JumpPointDistances[0] = 0; // north
                        continue;
                    }

                    jumpDistanceSoFar++;

                    if (jumpPointSeen)
                    {
                        // If we've seen a jump point heading left, then we can tell this node he's got a jump point coming up to his LEFT ( WEST )
                        node.JumpPointDistances[0] = jumpDistanceSoFar;
                    }
                    else
                    {
                        node.JumpPointDistances[0] = -jumpDistanceSoFar;   // Set wall distance
                    }

                    // If we just found a new jump point, then set everything up for this new jump point
                    if (node.JumpPointDirections[4])
                    {
                        jumpDistanceSoFar = 0;
                        jumpPointSeen = true;
                    }
                }

                jumpDistanceSoFar = -1;
                jumpPointSeen = false;

                for (int j = _gridSize.Row - 1; j > -1; j--)
                {
                    Node node = _nodes[j, i];

                    // If we've reach a wall, then reset everything :(
                    if (node.IsBlock(_size))
                    {
                        jumpDistanceSoFar = -1;
                        jumpPointSeen = false;
                        node.JumpPointDistances[4] = 0; // south
                        continue;
                    }

                    jumpDistanceSoFar++;

                    if (jumpPointSeen)
                    {
                        // If we've seen a jump point heading left, then we can tell this node he's got a jump point coming up to his LEFT ( WEST )
                        node.JumpPointDistances[4] = jumpDistanceSoFar;
                    }
                    else
                    {
                        node.JumpPointDistances[4] = -jumpDistanceSoFar;   // Set wall distance
                    }

                    // If we just found a new jump point, then set everything up for this new jump point
                    if (node.JumpPointDirections[0])
                    {
                        jumpDistanceSoFar = 0;
                        jumpPointSeen = true;
                    }
                }
            }
        }
        public void BuildDiagonalJumpPoints()
        {
            for (int i = 0; i < _gridSize.Row; i++)
            {
                for (int j = 0; j < _gridSize.Column; j++)
                {
                    if( _nodes[i, j].IsBlock(_size)) continue;

                    Node node = _nodes[i, j];

                    // NORTH_WEST�� ���
                    if (i == 0 || j == 0 ||
                        (IsOutOfRange(new Grid2D(i - 1, j)) == false && _nodes[i - 1, j].IsBlock(_size)) ||
                        (IsOutOfRange(new Grid2D(i, j - 1)) == false && _nodes[i, j - 1].IsBlock(_size)) ||
                        (IsOutOfRange(new Grid2D(i - 1, j - 1)) == false && _nodes[i - 1, j - 1].IsBlock(_size))
                    )
                    {
                        node.JumpPointDistances[7] = 0; // NORTH_WEST
                    }
                    else if(IsOutOfRange(new Grid2D(i - 1, j)) == false && _nodes[i - 1, j].IsBlock(_size) == false && // NORTH_WEST�� ����ְ�
                        IsOutOfRange(new Grid2D(i, j - 1)) == false && _nodes[i, j - 1].IsBlock(_size) == false &&
                        (_nodes[i - 1, j - 1].JumpPointDistances[0] > 0 ||
                        _nodes[i - 1, j - 1].JumpPointDistances[6] > 0)) // straight jump point�� ���� ���
                    {
                        node.JumpPointDistances[7] = 1; // NORTH_WEST
                    }
                    else
                    {
                        // Increment from last
                        int jumpDistance = _nodes[i - 1, j - 1].JumpPointDistances[7];

                        if (jumpDistance > 0)
                        {
                            node.JumpPointDistances[7] = 1 + jumpDistance;
                        }
                        else // ���� �����
                        {
                            node.JumpPointDistances[7] = -1 + jumpDistance;
                        }
                    }

                    // NORTH_EAST�� ���
                    if (i == 0 || j == _gridSize.Column - 1 ||
                        (IsOutOfRange(new Grid2D(i - 1, j)) == false && _nodes[i - 1, j].IsBlock(_size)) ||
                        (IsOutOfRange(new Grid2D(i, j + 1)) == false && _nodes[i, j + 1].IsBlock(_size)) ||
                        (IsOutOfRange(new Grid2D(i - 1, j + 1)) == false && _nodes[i - 1, j + 1].IsBlock(_size))
                    )
                    {
                        node.JumpPointDistances[1] = 0; // NORTH_EAST
                    }
                    else if (IsOutOfRange(new Grid2D(i - 1, j)) == false && _nodes[i - 1, j].IsBlock(_size) == false && // NORTH_EAST�� ����ְ�
                        IsOutOfRange(new Grid2D(i, j + 1)) == false && _nodes[i, j + 1].IsBlock(_size) == false &&
                        (_nodes[i - 1, j + 1].JumpPointDistances[0] > 0 ||
                        _nodes[i - 1, j + 1].JumpPointDistances[2] > 0)) // straight jump point�� ���� ���
                    {
                        node.JumpPointDistances[1] = 1; // NORTH_EAST
                    }
                    else
                    {
                        // Increment from last
                        int jumpDistance = _nodes[i - 1, j + 1].JumpPointDistances[1];

                        if (jumpDistance > 0)
                        {
                            node.JumpPointDistances[1] = 1 + jumpDistance;
                        }
                        else // ���� �����
                        {
                            node.JumpPointDistances[1] = -1 + jumpDistance;
                        }
                    }
                }
            }


            for (int i = _gridSize.Row - 1; i > -1; i--)
            {
                for (int j = 0; j < _gridSize.Column; j++)
                {
                    if (_nodes[i, j].IsBlock(_size)) continue;

                    Node node = _nodes[i, j];

                    // SOUTH_WEST�� ���
                    if (i == _gridSize.Row - 1 || j == 0 ||
                        (IsOutOfRange(new Grid2D(i + 1, j)) == false && _nodes[i + 1, j].IsBlock(_size)) ||
                        (IsOutOfRange(new Grid2D(i, j - 1)) == false && _nodes[i, j - 1].IsBlock(_size)) ||
                        (IsOutOfRange(new Grid2D(i + 1, j - 1)) == false && _nodes[i + 1, j - 1].IsBlock(_size))
                    )
                    {
                        node.JumpPointDistances[5] = 0; // SOUTH_WEST
                    }
                    else if (IsOutOfRange(new Grid2D(i + 1, j)) == false && _nodes[i + 1, j].IsBlock(_size) == false && // SOUTH_WEST�� ����ְ�
                        IsOutOfRange(new Grid2D(i, j - 1)) == false && _nodes[i, j - 1].IsBlock(_size) == false &&
                        (_nodes[i + 1, j - 1].JumpPointDistances[4] > 0 ||
                        _nodes[i + 1, j - 1].JumpPointDistances[6] > 0)) // straight jump point�� ���� ���
                    {
                        node.JumpPointDistances[5] = 1; // NORTH_WEST
                    }
                    else
                    {
                        // Increment from last
                        int jumpDistance = _nodes[i + 1, j - 1].JumpPointDistances[5];

                        if (jumpDistance > 0)
                        {
                            node.JumpPointDistances[5] = 1 + jumpDistance;
                        }
                        else // ���� �����
                        {
                            node.JumpPointDistances[5] = -1 + jumpDistance;
                        }
                    }

                    // SOUTH_EAST�� ���
                    if (i == _gridSize.Row - 1 || j == _gridSize.Column - 1 ||
                        (IsOutOfRange(new Grid2D(i + 1, j)) == false && _nodes[i + 1, j].IsBlock(_size)) ||
                        (IsOutOfRange(new Grid2D(i, j + 1)) == false && _nodes[i, j + 1].IsBlock(_size)) ||
                        (IsOutOfRange(new Grid2D(i + 1, j + 1)) == false && _nodes[i + 1, j + 1].IsBlock(_size))
                    )
                    {
                        node.JumpPointDistances[3] = 0; // SOUTH_EAST
                    }
                    else if (IsOutOfRange(new Grid2D(i + 1, j)) == false && _nodes[i + 1, j].IsBlock(_size) == false && // SOUTH_EAST�� ����ְ�
                        IsOutOfRange(new Grid2D(i, j + 1)) == false && _nodes[i, j + 1].IsBlock(_size) == false &&
                        (_nodes[i + 1, j + 1].JumpPointDistances[4] > 0 ||
                        _nodes[i + 1, j + 1].JumpPointDistances[2] > 0)) // straight jump point�� ���� ���
                    {
                        node.JumpPointDistances[3] = 1; // NORTH_EAST
                    }
                    else
                    {
                        // Increment from last
                        int jumpDistance = _nodes[i + 1, j + 1].JumpPointDistances[3];

                        if (jumpDistance > 0)
                        {
                            node.JumpPointDistances[3] = 1 + jumpDistance;
                        }
                        else // ���� �����
                        {
                            node.JumpPointDistances[3] = -1 + jumpDistance;
                        }
                    }
                }
            }


        }
    }
}