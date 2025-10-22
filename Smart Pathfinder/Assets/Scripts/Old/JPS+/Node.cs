using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace JPSPlus
{
    public enum Size
    {
        x1,
        x2,
        x3,
    }


    public class Node : IItem<Node>
    {
        public Node(Vector2 worldPos, Grid2D index, bool block)
        {
            _worldPos = worldPos;
            _index = index;
            _isBlock = block;

            StoredIndex = -1;
        }

        Vector2 _worldPos; // ���� ��ġ
        public Vector2 WorldPos { get { return _worldPos; } }

        Grid2D _index; // �׸��� �� �ε���
        public Grid2D Index { get { return _index; } }

        bool _isBlock; // Block ������� Ȯ��

        public bool IsBlock(Size size)
        {
            if (size == Size.x1)
            {
                return _isBlock;
            }
            else
            {
                return _haveNearBlockNode;
            }
        }


        bool _haveNearBlockNode; // �ֺ��� Block ��带 ������ �ִ��� Ȯ��
        public bool HaveNearBlockNode { set { _haveNearBlockNode = value; } }


        bool _isJumpPoint;
        public bool IsJumpPoint { get => _isJumpPoint; set => _isJumpPoint = value; }

        int[] _jumpPointDistances = new int[8]; // ���⺰ ��� �Ÿ�
        public int[] JumpPointDistances { get => _jumpPointDistances; set => _jumpPointDistances = value; }

        bool[] _jumpPointDirections = new bool[8]; // ���� ���� ����
        public bool[] JumpPointDirections { get => _jumpPointDirections; set => _jumpPointDirections = value; }

        public bool IsJumpPointComingFrom(int dir)
        {
            return _isJumpPoint && _jumpPointDirections[dir];
        }

        private int _directionFromParent;
        public int DirectionFromParent { get => _directionFromParent; set => _directionFromParent = value; }

        public Node[] NearNodes { get; set; } = new Node[8];
        public float[] NearNodeDistances { get; set; } = new float[8];
        public bool[] HaveNodes { get; set; } = new bool[8];

        // g�� ���� �������� �Ÿ�
        // h�� �� �������� �Ÿ�
        // f�� �� ���� ��ģ ��
        float g, h = 0;
        public float G { get { return g; } set { g = value; } }
        public float H { get { return h; } set { h = value; } }
        public float F { get { return g + h; } }

        public Node ParentNode { get; set; }
        public int StoredIndex { get; set; }

        public int CompareTo(Node other)
        {
            int compareValue = F.CompareTo(other.F);
            if (compareValue == 0) compareValue = H.CompareTo(other.H);
            return compareValue;
        }

        public void Dispose()
        {
            StoredIndex = -1;
            ParentNode = null;
        }
    }
}