using System.Collections.Generic;
using UnityEngine;

namespace PathfinderForTilemap
{
    [RequireComponent(typeof(AStarPathGrid))]
    public class AStarPathfinder : MonoBehaviour, IPathfinder
    {
        [Range(1, 2)]
        [SerializeField] float _targetWeight = 1f;
        [SerializeField] int _maxSearchDepth = 1000; // 최대 탐색 깊이 (무한루프 방지용)
        [SerializeField] bool _tracePath = false; // 디버그용 경로 추적 여부
        public bool TracePath { get => _tracePath; }

        int _searchDepth; // 탐색 깊이 (무한루프 방지용)

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

        // 가장 먼저 반올림을 통해 가장 가까운 노드를 찾는다.
        public List<Vector2> FindPath(Vector2 startPos, Vector2 targetPos, PathSize pathSize)
        {
            //// 리스트 초기화
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
                // 최대 탐색 깊이 초과 시 종료
                if (_searchDepth > _maxSearchDepth) break;

                // 시작의 경우 제외해줘야함
                AStarPathNode targetNode = _openList.ReturnMin();

                if (targetNode == endNode) // 목적지와 타겟이 같으면 끝
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

                _openList.DeleteMin(); // 해당 그리드 지워줌

                _closedList.Add(targetNode); // 해당 그리드 추가해줌
                if (_tracePath == true) _closedListToDebug.Add(targetNode.WorldPos);

                AddNearGridInList(targetNode, endNode, pathSize); // 주변 그리드를 찾아서 다시 넣어줌
            }

            // 이 경우는 경로를 찾지 못한 상황임
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

                if (_closedList.Contains(nearNode)) continue; // 통과하지 못하거나 닫힌 리스트에 있는 경우 다음 그리드 탐색

                // 이 부분 중요! --> 거리를 측정해서 업데이트 하지 않고 계속 더해주는 방식으로 진행해야함
                float moveCost = targetNode.G + Heuristic.GetEuclideanDistance(targetNode.Index, nearNode.Index) * nearNode.PathBias; 
                bool isOpenListContainNearGrid = _openList.Contain(nearNode);

                // 오픈 리스트에 있더라도 G 값이 변경된다면 다시 리셋해주기
                if (isOpenListContainNearGrid == false || moveCost < nearNode.G)
                {
                    // 여기서 grid 값 할당 필요
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