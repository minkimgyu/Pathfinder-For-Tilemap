using PathfinderForTilemap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AStarPathfinder))]
public class AStarPathDrawer : BaseDrawer
{
    AStarPathfinder _pathfinder;

    [SerializeField] bool _drawPath = false; // 디버그용 경로 그리기 여부

    [Header("Color")]
    [SerializeField] Color _openListColor = new Color(212f / 255f, 224f / 255f, 58f / 255f);
    [SerializeField] Color _closedListColor = new Color(255f / 255f, 92f / 255f, 76f / 255f);
    [SerializeField] Color _pathColor = Color.white;

    [Header("Size")]
    [SerializeField] float _nodeSize = 0.8f;

    public void Initialize(AStarPathfinder pathfinder)
    {
        _pathfinder = pathfinder;
    }

    protected override void DrawGrid()
    {
        if (_pathfinder == null) return;
        if(_pathfinder.TracePath == false || _drawPath == false) return;

        Vector2 currentNodeSize = Vector3.one * _nodeSize;

        for (int i = 0; i < _pathfinder.OpenListToDebug.Count; i++)
        {
            Gizmos.color = _openListColor;
            Gizmos.DrawCube(_pathfinder.OpenListToDebug[i], currentNodeSize);
        }

        for (int i = 0; i < _pathfinder.ClosedListToDebug.Count; i++)
        {
            Gizmos.color = _closedListColor;
            Gizmos.DrawCube(_pathfinder.ClosedListToDebug[i], currentNodeSize);
        }

        for (int i = 1; i < _pathfinder.PathToDebug.Count; i++)
        {
            Gizmos.color = _pathColor;
            Gizmos.DrawLine(_pathfinder.PathToDebug[i - 1], _pathfinder.PathToDebug[i]);
        }
    }
}
