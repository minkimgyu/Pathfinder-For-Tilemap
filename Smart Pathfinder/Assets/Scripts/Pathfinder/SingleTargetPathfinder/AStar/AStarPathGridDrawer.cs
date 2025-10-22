using PathfinderForTilemap;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PathfinderForTilemap
{
    public enum DrawType
    {
        None, // 그리지 않음
        BlockPass, // 막힌 지형과 통과 지형
        TerrainBias, // 우선도 및 패널티
    }

    [RequireComponent(typeof(AStarPathGrid))]
    public class AStarPathGridDrawer : BaseDrawer
    {
        [SerializeField] DrawType _drawType = DrawType.None;
        AStarPathGrid _grid;

        [Header("Color")]
        [SerializeField] Color _blockNodeColor = new Color(224f / 255f, 73f / 255f, 58f / 255f, 0.3f);
        [SerializeField] Color _passNodeColor = new Color(76f / 255f, 224f / 255f, 255f / 255f, 0.3f);

        [SerializeField] Color _priorityColor = new Color(76f / 255f, 101f / 255f, 255f / 255f);
        [SerializeField] Color _penaltyColor = new Color(236f / 255f, 57f / 255f, 96f / 255f);
        [SerializeField] Color _lineColor = Color.white;

        [Header("Size")]
        [SerializeField] float _nodeSize = 0.8f;

        GUIStyle _fontStyle;

        public void Initialize(AStarPathGrid grid)
        {
            _grid = grid;
            _fontStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter, // ✅ 중앙 정렬
                normal = { textColor = Color.white },
                fontSize = 12
            };
        }

        protected override void DrawGrid()
        {
            if (_grid == null) return;

            Grid2D grid2D = _grid.GetGridSize();

            switch (_drawType)
            {
                case DrawType.BlockPass:
                    for (int i = 0; i < grid2D.Row; i++)
                    {
                        for (int j = 0; j < grid2D.Column; j++)
                        {
                            AStarPathNode aStarPathNode = _grid.GetPathNode(new Grid2D(i, j));

                            if (aStarPathNode.Block)
                            {
                                Gizmos.color = _blockNodeColor;
                            }
                            else
                            {
                                Gizmos.color = _passNodeColor;
                            }

                            Gizmos.DrawCube(aStarPathNode.WorldPos, Vector3.one * _nodeSize);
                        }
                    }
                    break;
                case DrawType.TerrainBias:

                    for (int i = 0; i < grid2D.Row; i++)
                    {
                        for (int j = 0; j < grid2D.Column; j++)
                        {
                            AStarPathNode aStarPathNode = _grid.GetPathNode(new Grid2D(i, j));

                            if (aStarPathNode.PathBias != 1)
                            {
                                Vector3 pos = aStarPathNode.WorldPos;
                                Color boxColor;

                                if (aStarPathNode.TerrainWeight >= 0)
                                {
                                    boxColor = new Color(_penaltyColor.r, _penaltyColor.g, _penaltyColor.b, aStarPathNode.TerrainWeight);
                                }
                                else
                                {
                                    boxColor = new Color(_priorityColor.r, _priorityColor.g, _priorityColor.b, -aStarPathNode.TerrainWeight);
                                }

                                float halfSize = _nodeSize / 2;

                                Handles.DrawSolidRectangleWithOutline(
                                    new Vector3[]
                                    {
                                        pos + new Vector3(-halfSize, -halfSize, 0),
                                        pos + new Vector3(-halfSize, halfSize, 0),
                                        pos + new Vector3(halfSize, halfSize, 0),
                                        pos + new Vector3(halfSize, -halfSize, 0)
                                    },
                                    boxColor,   // 내부 색상 (RGBA)
                                    _lineColor  // 외곽선 색상
                                );

                                // 또는 SphereHandleCap 등으로 가이드 점 표시
                                Handles.color = Color.white;
                                Handles.Label(aStarPathNode.WorldPos, aStarPathNode.TerrainWeight.ToString("0.##"), _fontStyle);
                            }
                        }
                    }

                    break;
                default:
                    break;
            }
        }
    }
}