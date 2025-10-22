using System;

namespace PathfinderForTilemap
{
    public enum PathSize
    {
        Size1x1, // 1x1
        Size3x3, // 3x3
    }

    public interface IPathNode<T> : IComparable<T>
    {
        public int StoredIndex { get; set; } // �������� ���� �ε���
        public void Dispose(); // ��� �Ҹ� �Լ�
    }
}