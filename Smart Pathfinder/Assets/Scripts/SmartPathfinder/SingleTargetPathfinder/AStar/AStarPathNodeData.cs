using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfinderForTilemap
{
    [System.Serializable]
    public struct AStarPathNodeData
    {
        [SerializeField] Vector2 _worldPos; // 실제 위치
        [SerializeField] Grid2D _index; // 그리드 상 인덱스
        [SerializeField] bool _block; // 막힘 여부
        [SerializeField] float _terrainWeight; // 이동 비용에 "지역 가중치" 반영
        [SerializeField] List<int> _nearNodeIndexesSize1x1; // 인접 노드 인덱스들
        [SerializeField] List<int> _nearNodeIndexesSize3x3; // 인접 노드 인덱스들

        public AStarPathNodeData(
            Vector2 worldPos,
            Grid2D index,
            bool block,
            float terrainWeight,
            List<int> nearNodeIndexesSize1x1,
            List<int> nearNodeIndexesSize3x3)
        {
            _worldPos = worldPos;
            _index = index;
            _block = block;
            _terrainWeight = terrainWeight;
            _nearNodeIndexesSize1x1 = nearNodeIndexesSize1x1;
            _nearNodeIndexesSize3x3 = nearNodeIndexesSize3x3;
        }

        public Vector2 WorldPos { get => _worldPos; }
        public Grid2D Index { get => _index; }
        public bool Block { get => _block; }
        public float TerrainWeight { get => _terrainWeight; }

        public float PathBias
        {
            get { return Mathf.Max(0.1f, 1 + TerrainWeight); }
        }

        public List<int> NearNodeIndexesSize1x1 { get => _nearNodeIndexesSize1x1; }
        public List<int> NearNodeIndexesSize3x3 { get => _nearNodeIndexesSize3x3; }
    }
}
