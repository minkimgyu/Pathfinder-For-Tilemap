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
        public int StoredIndex { get; set; } // 힙에서의 저장 인덱스
        public void Dispose(); // 노드 소멸 함수
    }
}