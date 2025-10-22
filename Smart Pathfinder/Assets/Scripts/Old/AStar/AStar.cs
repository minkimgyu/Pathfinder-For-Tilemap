using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace AStar
{
    public class AStar : MonoBehaviour
    {
        Func<Vector2, Grid2D> ReturnNodeIndex;
        Func<Grid2D, Node> ReturnNode;

        const int maxSize = 1000;

        Heap<Node> _openList = new Heap<Node>(maxSize);
        HashSet<Node> _closedList = new HashSet<Node>();

        [SerializeField] int _awaitDuration = 30;

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
        public async Task<List<Vector2>> FindPath(Vector2 startPos, Vector2 targetPos)
        {
            _openListPoints.Clear();
            _closeListPoints.Clear();
            //// ����Ʈ �ʱ�ȭ
            _openList.Clear();
            _closedList.Clear();

            Grid2D startIndex = ReturnNodeIndex(startPos);
            Grid2D endIndex = ReturnNodeIndex(targetPos);

            Node startNode = ReturnNode(startIndex);
            Node endNode = ReturnNode(endIndex);

            if (startNode == null || endNode == null) return null;

            _openList.Insert(startNode);

            while (_openList.Count > 0)
            {
                await Task.Delay(_awaitDuration);

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

                _closeListPoints.Add(targetNode.WorldPos);
                _closedList.Add(targetNode); // �ش� �׸��� �߰�����
                AddNearGridInList(targetNode, endNode); // �ֺ� �׸��带 ã�Ƽ� �ٽ� �־���
            }

            // �� ���� ��θ� ã�� ���� ��Ȳ��
            return null;
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
                float moveCost = targetNode.G + Vector2.Distance(targetNode.WorldPos, nearNode.WorldPos);
                bool isOpenListContainNearGrid = _openList.Contain(nearNode);

                // ���� ����Ʈ�� �ִ��� G ���� ����ȴٸ� �ٽ� �������ֱ�
                if (isOpenListContainNearGrid == false || moveCost < nearNode.G)
                {
                    // ���⼭ grid �� �Ҵ� �ʿ�
                    nearNode.G = moveCost;
                    nearNode.H = Vector2.Distance(nearNode.WorldPos, endNode.WorldPos); // targetNode
                    nearNode.ParentNode = targetNode;
                }

                if (isOpenListContainNearGrid == false)
                {
                    _openListPoints.Add(nearNode.WorldPos);
                    _openList.Insert(nearNode);
                }
            }
        }

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
    }
}