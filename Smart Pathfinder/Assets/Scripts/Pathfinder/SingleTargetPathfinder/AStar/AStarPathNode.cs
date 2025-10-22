using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfinderForTilemap
{
    public class AStarPathNode : IPathNode<AStarPathNode>
    {
        public AStarPathNode(
            Vector2 worldPos,
            Grid2D index,
            bool block,
            float terrainWeight = 0,
            Dictionary<PathSize, List<int>> nearNodeIndexes = null)
        {
            _worldPos = worldPos;
            _index = index;
            _block = block;

            _terrainWeight = terrainWeight;

            if(nearNodeIndexes == null)
            {
                _nearNodeIndexes = new Dictionary<PathSize, List<int>>();
                foreach (PathSize size in System.Enum.GetValues(typeof(PathSize)))
                {
                    _nearNodeIndexes[size] = new List<int>();
                }
            }
            else
            {
                _nearNodeIndexes = nearNodeIndexes;
            }

            StoredIndex = -1;
        }

        Vector2 _worldPos; // ���� ��ġ
        public Vector2 WorldPos { get { return _worldPos; } }

        Grid2D _index; // �׸��� �� �ε���
        public Grid2D Index { get { return _index; } }

        bool _block; // ���� ����
        public bool Block { get { return _block; } set { _block = value; } }

        Dictionary<PathSize, List<int>> _nearNodeIndexes;
        public Dictionary<PathSize, List<int>> NearNodeIndexes 
        { 
            get { return _nearNodeIndexes; } 
            set { _nearNodeIndexes = value; }
        }

        // g�� ���� �������� �Ÿ�
        // h�� �� �������� �Ÿ�
        // f�� �� ���� ��ģ ��
        float _g, _h = 0;

        // ������ -1 ~ +1����
        float _terrainWeight = 0; // �̵� ��뿡 "�켱 ���� ����ġ" �ݿ�
        public float TerrainWeight 
        {
            get 
            { 
                if( _terrainWeight < -1) return -1;
                else if( _terrainWeight > 1) return 1;
                else return _terrainWeight;
            }
            set { _terrainWeight = value; } 
        }
       
        public float PathBias
        {
            get { return Mathf.Max(0.1f, 1 + TerrainWeight); }
        }

        public float G { get { return _g; } set { _g = value; } }
        public float H { get { return _h; } set { _h = value; } }
        public float F { get { return _g + _h; } }

        public AStarPathNode ParentNode { get; set; }
        public int StoredIndex { get; set; }

        public int CompareTo(AStarPathNode other)
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