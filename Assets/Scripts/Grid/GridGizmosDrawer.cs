using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gameplay.Grid
{
    [RequireComponent(typeof(GridSceneReferences))]
    public class GridGizmosDrawer : MonoBehaviour
    {
        [SerializeField] private GridConfig _editorConfig;
        
        private GridSceneReferences _references;


        private const float BaseThickness = 0.2f;
        private const float GizmoSize = 0.9f;
        
        private static readonly Color PathColor = new Color(1f, 0.9f, 0f, 0.5f);
        private static readonly Color ObstacleColor = new Color(1f, 0f, 0f, 0.5f);
        private static readonly Color SpawnColor = new Color(1f, 0f, 1f, 0.6f);
        private static readonly Color BaseColor = new Color(0f, 0f, 1f, 0.6f);
        private static readonly Color GroundColor = new Color(0f, 1f, 1f, 0.4f);

        private const string SpawnPrefix = "Spawn_{0}_{1}";
        private const string BasePrefix = "Base_{0}_{1}";

        public GridConfig EditorConfig => _editorConfig;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_references == null)
            {
                _references = GetComponent<GridSceneReferences>();
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying || _editorConfig == null || _references == null) return;

            float spacing = _references.Spacing;
            float elevationStep = _references.ElevationStep;

            for (int x = 0; x < _editorConfig.width; x++)
            {
                for (int z = 0; z < _editorConfig.height; z++)
                {
                    GridCellData cellData = _editorConfig.GetCellData(x, z);
                    
                    Gizmos.color = cellData.type switch
                    {
                        NodeType.Path => PathColor,
                        NodeType.Obstacle => ObstacleColor,
                        NodeType.Spawn => SpawnColor,
                        NodeType.Base => BaseColor,
                        _ => GroundColor
                    };
                    
                    float addedHeight = cellData.elevation * elevationStep;
                    Vector3 center = new Vector3(x * spacing, addedHeight / 2f, z * spacing);
                    Vector3 size = new Vector3(GizmoSize, BaseThickness + addedHeight, GizmoSize);

                    Gizmos.DrawCube(center, size);
                    Gizmos.DrawWireCube(center, size);

                    DrawLabels(cellData.type, center, x, z);
                }
            }
        }

        private void DrawLabels(NodeType type, Vector3 center, int x, int z)
        {
            if (type == NodeType.Spawn || type == NodeType.Base)
            {
                GUIStyle style = new GUIStyle 
                { 
                    fontStyle = FontStyle.Bold, 
                    alignment = TextAnchor.MiddleCenter 
                };
                style.normal.textColor = Color.white;
                
                string labelText = type == NodeType.Spawn 
                    ? string.Format(SpawnPrefix, x, z) 
                    : string.Format(BasePrefix, x, z);
                    
                Handles.Label(center + Vector3.up, labelText, style);
            }
        }
#endif
    }
}