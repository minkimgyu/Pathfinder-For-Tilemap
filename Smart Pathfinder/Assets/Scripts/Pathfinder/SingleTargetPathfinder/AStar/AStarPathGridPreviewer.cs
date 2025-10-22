using UnityEditor;
using UnityEngine;
using static UnityEditor.ShaderData;

namespace PathfinderForTilemap
{
    public class AStarPathGridPreviewer : BaseDrawer
    {
        [SerializeField] DrawType _drawType = DrawType.None;
        [SerializeField] PathNodeSO _pathNodeSO;

        [Header("Color")]
        [SerializeField] Color _blockNodeColor = new Color(224f / 255f, 73f / 255f, 58f / 255f, 0.3f);
        [SerializeField] Color _passNodeColor = new Color(76f / 255f, 224f / 255f, 255f / 255f, 0.3f);

        [SerializeField] Color _priorityColor = new Color(76f / 255f, 101f / 255f, 255f / 255f);
        [SerializeField] Color _penaltyColor = new Color(236f / 255f, 57f / 255f, 96f / 255f);
        [SerializeField] Color _lineColor = Color.white;

        [Header("Size")]
        [SerializeField] float _nodeSize = 0.8f;

        protected override void DrawGrid()
        {
#if UNITY_EDITOR

            if (Application.isPlaying) return;
            if (_pathNodeSO == null) return;
            if (_pathNodeSO.AStarPathNodeData == null) return;
            if (_pathNodeSO.AStarPathNodeData.Length == 0 ) return;
            if (_pathNodeSO.AStarPathNodeData[0]._column.Length == 0 ) return;

            Grid2D grid2D = new Grid2D(_pathNodeSO.AStarPathNodeData.Length, _pathNodeSO.AStarPathNodeData[0]._column.Length);

            GUIStyle fontStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter, // ✅ 중앙 정렬
                normal = { textColor = Color.white },
                fontSize = 12
            };

            switch (_drawType)
            {
                case DrawType.BlockPass:
                    for (int i = 0; i < grid2D.Row; i++)
                    {
                        for (int j = 0; j < grid2D.Column; j++)
                        {
                            AStarPathNodeData data = _pathNodeSO.AStarPathNodeData[i]._column[j];

                            if (data.Block)
                            {
                                Gizmos.color = _blockNodeColor;
                            }
                            else
                            {
                                Gizmos.color = _passNodeColor;
                            }

                            Gizmos.DrawCube(data.WorldPos, Vector3.one * _nodeSize);
                        }
                    }
                    break;
                case DrawType.TerrainBias:

                    for (int i = 0; i < grid2D.Row; i++)
                    {
                        for (int j = 0; j < grid2D.Column; j++)
                        {
                            AStarPathNodeData data = _pathNodeSO.AStarPathNodeData[i]._column[j];

                            if (data.PathBias != 1)
                            {
                                Vector3 pos = data.WorldPos;
                                Color boxColor;

                                if (data.TerrainWeight >= 0)
                                {
                                    boxColor = new Color(_penaltyColor.r, _penaltyColor.g, _penaltyColor.b, data.TerrainWeight);
                                }
                                else
                                {
                                    boxColor = new Color(_priorityColor.r, _priorityColor.g, _priorityColor.b, -data.TerrainWeight);
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
                                   _lineColor   // 외곽선 색상
                                );

                                // 또는 SphereHandleCap 등으로 가이드 점 표시
                                Handles.color = Color.white;
                                Handles.Label(data.WorldPos, data.TerrainWeight.ToString("0.##"), fontStyle);
                            }
                        }
                    }

                    break;
                default:
                    break;
            }
#endif
        }
    }
}