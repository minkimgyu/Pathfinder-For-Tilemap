//using UnityEngine;

//#if UNITY_EDITOR
//using UnityEditor;
//#endif

//public class GridLabelDrawer : MonoBehaviour
//{
//#if UNITY_EDITOR
//    private void OnDrawGizmos()
//    {
//        if (!Application.isPlaying) // ������ ����� ���� ����
//        {
//            Handles.BeginGUI();

//            for (int y = 0; y < 10; y++)
//            {
//                for (int x = 0; x < 10; x++)
//                {
//                    Vector3 worldPos = new Vector3(x + 0.5f, y + 0.5f, 0f);
//                    Vector3 screenPos = HandleUtility.WorldToGUIPoint(worldPos);

//                    GUIStyle style = new GUIStyle()
//                    {
//                        fontSize = 14,
//                        normal = new GUIStyleState() { textColor = Color.white }
//                    };

//                    // GUI ��ǥ��� Y���� �Ʒ��� ���ϹǷ� ��ȯ �ʿ� ����
//                    GUI.Label(new Rect(screenPos.x, screenPos.y, 100, 20), $"({x},{y})", style);
//                }
//            }

//            Handles.EndGUI();
//        }
//    }
//#endif
//}
