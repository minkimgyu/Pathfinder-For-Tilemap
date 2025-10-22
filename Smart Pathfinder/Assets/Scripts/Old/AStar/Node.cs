using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AStar
{
    public class Node : IItem<Node>
    {
        public Node(Vector2 worldPos, Grid2D index, bool block)
        {
            _worldPos = worldPos;
            _index = index;
            _block = block;

            StoredIndex = -1;
        }

        Vector2 _worldPos; // ���� ��ġ
        public Vector2 WorldPos { get { return _worldPos; } }

        Grid2D _index; // �׸��� �� �ε���
        public Grid2D Index { get { return _index; } }

        bool _block;
        public bool Block { get { return _block; } }

        public List<Node> NearNodes { get; set; } = new List<Node>(); // small ��

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