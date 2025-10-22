using System.Collections.Generic;
using UnityEngine;

namespace PathfinderForTilemap
{
    [RequireComponent(typeof(AStarPathGrid))]
    public class AStarPathfinder : MonoBehaviour, IPathfinder
    {
        [Range(1, 2)]
        [SerializeField] float _targetWeight = 1f;
        [SerializeField] int _maxSearchDepth = 1000; // �ִ� Ž�� ���� (���ѷ��� ������)
        [SerializeField] bool _tracePath = false; // ����׿� ��� ���� ����
        public bool TracePath { get => _tracePath; }

        int _searchDepth; // Ž�� ���� (���ѷ��� ������)

        Heap<AStarPathNode> _openList;
        HashSet<AStarPathNode> _closedList;

        List<Vector2> _pathToDebug;
        List<Vector2> _openListToDebug;
        List<Vector2> _closedListToDebug;

        public List<Vector2> PathToDebug { get => _pathToDebug; }
        public List<Vector2> OpenListToDebug { get => _openListToDebug; }
        public List<Vector2> ClosedListToDebug { get => _closedListToDebug; }

        IPathGrid<AStarPathNode> _gridComponent;
        AStarPathDrawer _drawer;

        public void Initialize(IPathGrid<AStarPathNode> gridComponent)
        {
            _gridComponent = gridComponent;
            _searchDepth = 0;

            _openList = new Heap<AStarPathNode>(_maxSearchDepth);
            _closedList = new HashSet<AStarPathNode>();

            _pathToDebug = new List<Vector2>();
            _openListToDebug = new List<Vector2>();
            _closedListToDebug = new List<Vector2>();

            _drawer = GetComponent<AStarPathDrawer>();
            _drawer.Initialize(this);
        }

        List<Vector2> ConvertNodeToV2(Stack<AStarPathNode> stackNode)
        {
            List<Vector2> points = new List<Vector2>();
            while (stackNode.Count > 0)
            {
                AStarPathNode node = stackNode.Pop();
                points.Add(node.WorldPos);
                if (_tracePath == true) _pathToDebug.Add(node.WorldPos);
            }

            return points;
        }

        // ���� ���� �ݿø��� ���� ���� ����� ��带 ã�´�.
        public List<Vector2> FindPath(Vector2 startPos, Vector2 targetPos, PathSize pathSize)
        {
            //// ����Ʈ �ʱ�ȭ
            _openList.Clear();
            _closedList.Clear();
            _searchDepth = 0;

            if (_tracePath == true)
            {
                _pathToDebug.Clear();
                _openListToDebug.Clear();
                _closedListToDebug.Clear();
            }

            Grid2D startIndex = _gridComponent.GetPathNodeIndex(startPos);
            Grid2D endIndex = _gridComponent.GetPathNodeIndex(targetPos);

            AStarPathNode startNode = _gridComponent.GetPathNode(startIndex);
            AStarPathNode endNode = _gridComponent.GetPathNode(endIndex);

            if (startNode == null || endNode == null) return null;

            _openList.Insert(startNode);
            _searchDepth++;
            if (_tracePath == true) _openListToDebug.Add(startNode.WorldPos);

            while (_openList.Count > 0)
            {
                // �ִ� Ž�� ���� �ʰ� �� ����
                if (_searchDepth > _maxSearchDepth) break;

                // ������ ��� �����������
                AStarPathNode targetNode = _openList.ReturnMin();

                if (targetNode == endNode) // �������� Ÿ���� ������ ��
                {
                    Stack<AStarPathNode> finalList = new Stack<AStarPathNode>();

                    AStarPathNode TargetCurNode = targetNode;
                    while (TargetCurNode != startNode)
                    {
                        finalList.Push(TargetCurNode);
                        TargetCurNode = TargetCurNode.ParentNode;
                    }

                    finalList.Push(startNode);

                    return ConvertNodeToV2(finalList);
                }

                _openList.DeleteMin(); // �ش� �׸��� ������

                _closedList.Add(targetNode); // �ش� �׸��� �߰�����
                if (_tracePath == true) _closedListToDebug.Add(targetNode.WorldPos);

                AddNearGridInList(targetNode, endNode, pathSize); // �ֺ� �׸��带 ã�Ƽ� �ٽ� �־���
            }

            // �� ���� ��θ� ã�� ���� ��Ȳ��
            return null;
        }

        void AddNearGridInList(AStarPathNode targetNode, AStarPathNode endNode, PathSize pathSize)
        {
            if (targetNode.NearNodeIndexes[pathSize] == null) return;

            List<int> nearNodeIndexes = targetNode.NearNodeIndexes[pathSize];

            for (int i = 0; i < nearNodeIndexes.Count; i++)
            {
                Grid2D grid = GridUtility.NearIndexes[nearNodeIndexes[i]];
                AStarPathNode nearNode = _gridComponent.GetPathNode(new Grid2D(targetNode.Index.Row + grid.Row, targetNode.Index.Column + grid.Column));

                if (_closedList.Contains(nearNode)) continue; // ������� ���ϰų� ���� ����Ʈ�� �ִ� ��� ���� �׸��� Ž��

                // �� �κ� �߿�! --> �Ÿ��� �����ؼ� ������Ʈ ���� �ʰ� ��� �����ִ� ������� �����ؾ���
                float moveCost = targetNode.G + Heuristic.GetEuclideanDistance(targetNode.Index, nearNode.Index) * nearNode.PathBias; 
                bool isOpenListContainNearGrid = _openList.Contain(nearNode);

                // ���� ����Ʈ�� �ִ��� G ���� ����ȴٸ� �ٽ� �������ֱ�
                if (isOpenListContainNearGrid == false || moveCost < nearNode.G)
                {
                    // ���⼭ grid �� �Ҵ� �ʿ�
                    nearNode.G = moveCost;
                    nearNode.H = Heuristic.GetEuclideanDistance(nearNode.Index, endNode.Index) * _targetWeight;
                    nearNode.ParentNode = targetNode;
                }

                if (isOpenListContainNearGrid == false)
                {
                    _openList.Insert(nearNode);
                    _searchDepth++;
                    if (_tracePath == true) _openListToDebug.Add(nearNode.WorldPos);
                }
            }
        }
    }
}