using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Unity.Profiling;

namespace PathfinderForTilemap
{
    public class GameMode : MonoBehaviour
    {
        [SerializeField] Transform _startPoint;
        [SerializeField] Transform _endPoint;

        [SerializeField] AStarPathfinder _aStar;
        [SerializeField] AStarPathGrid _gridComponent;
        [SerializeField] PathSize _pathSize;

        private ProfilerMarker myMarker2 = new ProfilerMarker("Pathfinding");

        bool _nowFind = false;

        private void Start()
        {
            _gridComponent.Initialize();
            _aStar.Initialize(_gridComponent);
        }

        private void Update()
        {
            if (_nowFind == true) return;

            if (Input.GetMouseButtonDown(0))
            {
                _nowFind = true;

                myMarker2.Begin();
                // 측정 구간

                _aStar.FindPath(_startPoint.position, _endPoint.position, _pathSize);

                myMarker2.End();

                _nowFind = false;
            }
        }
    }
}