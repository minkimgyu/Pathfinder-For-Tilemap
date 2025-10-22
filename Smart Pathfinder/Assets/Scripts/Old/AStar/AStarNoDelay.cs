#define Draw_Progress // Ȱ��ȭ

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;



namespace AStar
{
    public class AStarNoDelay : MonoBehaviour
    {
        Func<Vector2, Grid2D> ReturnNodeIndex;
        Func<Grid2D, Node> ReturnNode;

        const int maxSize = 1000;

        Heap<Node> _openList = new Heap<Node>(maxSize);
        HashSet<Node> _closedList = new HashSet<Node>();

        bool _drawProgress = true;

        public void Initialize(GridComponent gridComponent)
        {
            ReturnNodeIndex = gridComponent.ReturnNodeIndex;
            ReturnNode = gridComponent.ReturnNode;
        }

        List<Vector2> ConvertNodeToV2(Stack<Node> stackNode)
        {
            List<Vector2> points = new List<Vector2>();
            while (stackNode.Count > 0)
            {
                Node node = stackNode.Pop();
                points.Add(node.WorldPos);
            }

            return points;
        }

        // ���� ���� �ݿø��� ���� ���� ����� ��带 ã�´�.
        public List<Vector2> FindPath(Vector2 startPos, Vector2 targetPos)
        {

#if Draw_Progress
            _openListPoints.Clear();
            _closeListPoints.Clear();
#endif
            //// ����Ʈ �ʱ�ȭ
            _openList.Clear();
            _closedList.Clear();

            Grid2D startIndex = ReturnNodeIndex(startPos);
            Grid2D endIndex = ReturnNodeIndex(targetPos);

            Node startNode = ReturnNode(startIndex);
            Node endNode = ReturnNode(endIndex);

            if (startNode == null || endNode == null) return null;

            _openList.Insert(startNode);

#if Draw_Progress
            _openListPoints.Add(startNode.WorldPos);
#endif

            while (_openList.Count > 0)
            {
                // ������ ��� �����������
                Node targetNode = _openList.ReturnMin();

                if (targetNode == endNode) // �������� Ÿ���� ������ ��
                {
                    Stack<Node> finalList = new Stack<Node>();

                    Node TargetCurNode = targetNode;
                    while (TargetCurNode != startNode)
                    {
                        finalList.Push(TargetCurNode);
                        TargetCurNode = TargetCurNode.ParentNode;
                    }

                    finalList.Push(startNode);

                    return ConvertNodeToV2(finalList);
                }

                _openList.DeleteMin(); // �ش� �׸��� ������
#if Draw_Progress
                _closeListPoints.Add(targetNode.WorldPos);
#endif
                _closedList.Add(targetNode); // �ش� �׸��� �߰�����
                AddNearGridInList(targetNode, endNode); // �ֺ� �׸��带 ã�Ƽ� �ٽ� �־���
            }

            // �� ���� ��θ� ã�� ���� ��Ȳ��
            return null;
        }

        static readonly float SQRT_2_MINUS_1 = Mathf.Sqrt(2) - 1.0f;

        internal static int OctileHeuristic(int curr_row, int curr_column, int goal_row, int goal_column)
        {
            int heuristic;
            int row_dist = Mathf.Abs(goal_row - curr_row);
            int column_dist = Mathf.Abs(goal_column - curr_column); // �޸���ƽ ���� ������ ������ ���� �߻�

            heuristic = (int)(Mathf.Max(row_dist, column_dist) + SQRT_2_MINUS_1 * Mathf.Min(row_dist, column_dist));

            return heuristic;
        }

        void AddNearGridInList(Node targetNode, Node endNode)
        {
            List<Node> nearNodes = targetNode.NearNodes;
            if (nearNodes == null) return;

            for (int i = 0; i < nearNodes.Count; i++)
            {
                Node nearNode = nearNodes[i];
                //bool nowHave = HaveBlockNodeInNearPosition(nearNode.Index, size);
                //if (nowHave) continue;

                // ���⼭ bfs ������ �ֺ� 3X3 ĭ�� �̵� �Ұ����� ��ΰ� �ִٸ� �ٽ� �̾��ش�.
                // ���� ��� ��带 �� ���� ��� ���Ͻ�Ų��.

                if (_closedList.Contains(nearNode)) continue; // ������� ���ϰų� ���� ����Ʈ�� �ִ� ��� ���� �׸��� Ž��

                // �� �κ� �߿�! --> �Ÿ��� �����ؼ� ������Ʈ ���� �ʰ� ��� �����ִ� ������� �����ؾ���


                

                float moveCost = targetNode.G + OctileHeuristic(targetNode.Index.Row, targetNode.Index.Column, nearNode.Index.Row, nearNode.Index.Column); // MathF.Abs(targetNode.WorldPos.x - nearNode.WorldPos.x) + MathF.Abs(targetNode.WorldPos.y - nearNode.WorldPos.y); // Vector2.Distance(targetNode.WorldPos, nearNode.WorldPos);
                bool isOpenListContainNearGrid = _openList.Contain(nearNode);

                // ���� ����Ʈ�� �ִ��� G ���� ����ȴٸ� �ٽ� �������ֱ�
                if (isOpenListContainNearGrid == false || moveCost < nearNode.G)
                {
                    // ���⼭ grid �� �Ҵ� �ʿ�
                    nearNode.G = moveCost;
                    nearNode.H = OctileHeuristic(nearNode.Index.Row, nearNode.Index.Column, endNode.Index.Row, endNode.Index.Column); //MathF.Abs(nearNode.WorldPos.x - endNode.WorldPos.x) + MathF.Abs(nearNode.WorldPos.y - endNode.WorldPos.y); // Vector2.Distance(nearNode.WorldPos, endNode.WorldPos); //  //  // targetNode
                    nearNode.ParentNode = targetNode;
                }

                if (isOpenListContainNearGrid == false)
                {
#if Draw_Progress
                    _openListPoints.Add(nearNode.WorldPos);
#endif
                    _openList.Insert(nearNode);
                }
            }
        }

#if Draw_Progress

        List<Vector2> _closeListPoints = new List<Vector2>();
        List<Vector2> _openListPoints = new List<Vector2>();

        private void OnDrawGizmos()
        {
            for (int i = 0; i < _openListPoints.Count; i++)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(_openListPoints[i], new Vector2(0.8f, 0.8f));
            }

            for (int i = 0; i < _closeListPoints.Count; i++)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(_closeListPoints[i], new Vector2(0.8f, 0.8f));
            }
        }
#endif
    }
}