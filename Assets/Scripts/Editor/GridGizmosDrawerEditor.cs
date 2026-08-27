using UnityEngine;
using UnityEditor;
using Gameplay.Grid;

namespace Gameplay.Editor
{
    [CustomEditor(typeof(GridGizmosDrawer))]
    public class GridGizmosDrawerEditor : UnityEditor.Editor
    {
        public enum BrushMode { PaintGround, PaintPath, PaintObstacle, PaintSpawn, PaintBase, RaiseElevation, LowerElevation }
        private BrushMode _currentBrushMode = BrushMode.PaintPath;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GridGizmosDrawer drawer = (GridGizmosDrawer)target;
            GridConfig config = drawer.EditorConfig;

            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("🛠 Создать / Сбросить матрицу сетки", GUILayout.Height(30)))
            {
                if (config != null)
                {
                    config.rows = new GridRow[config.width];
                    for (int x = 0; x < config.width; x++)
                    {
                        config.rows[x].columns = new GridCellData[config.height];
                        for (int z = 0; z < config.height; z++)
                        {
                            config.rows[x].columns[z] = new GridCellData { elevation = 0, type = NodeType.Ground };
                        }
                    }
                    EditorUtility.SetDirty(config);
                    SceneView.RepaintAll();
                    Debug.Log("<color=green>[Editor] Сетка успешно инициализирована!</color>");
                }
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("🖌 Инструменты Левел-Дизайнера", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(_currentBrushMode == BrushMode.PaintPath, "Дорога (Желтая)", "Button")) _currentBrushMode = BrushMode.PaintPath;
            if (GUILayout.Toggle(_currentBrushMode == BrushMode.PaintGround, "Земля (Голубая)", "Button")) _currentBrushMode = BrushMode.PaintGround;
            if (GUILayout.Toggle(_currentBrushMode == BrushMode.PaintObstacle, "Препятствие (Красное)", "Button")) _currentBrushMode = BrushMode.PaintObstacle;
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(_currentBrushMode == BrushMode.PaintSpawn, "Спавн (Пурпур)", "Button")) _currentBrushMode = BrushMode.PaintSpawn;
            if (GUILayout.Toggle(_currentBrushMode == BrushMode.PaintBase, "База (Синий)", "Button")) _currentBrushMode = BrushMode.PaintBase;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(_currentBrushMode == BrushMode.RaiseElevation, "Поднять (+)", "Button")) _currentBrushMode = BrushMode.RaiseElevation;
            if (GUILayout.Toggle(_currentBrushMode == BrushMode.LowerElevation, "Опустить (-)", "Button")) _currentBrushMode = BrushMode.LowerElevation;
            GUILayout.EndHorizontal();
            
            EditorGUILayout.HelpBox("Выделите объект с GridGizmosDrawer и рисуйте по сетке в окне Scene. ЛКМ = Рисовать.", MessageType.Info);
        }

        private void OnSceneGUI()
        {
            GridGizmosDrawer drawer = (GridGizmosDrawer)target;
            GridConfig config = drawer.EditorConfig;
            
            // Получаем ссылки на сцену для расчета рейкаста
            GridSceneReferences refs = drawer.GetComponent<GridSceneReferences>();

            if (config == null || config.rows == null || config.rows.Length == 0 || refs == null) return;

            Event e = Event.current;

            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
            {
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                if (groundPlane.Raycast(ray, out float enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    float spacing = refs.Spacing;
                    float halfSpacing = spacing / 2f;
                    
                    int x = Mathf.FloorToInt((hitPoint.x + halfSpacing) / spacing);
                    int z = Mathf.FloorToInt((hitPoint.z + halfSpacing) / spacing);

                    if (x >= 0 && x < config.width && z >= 0 && z < config.height)
                    {
                        ApplyBrush(config, x, z);
                    }
                }

                GUIUtility.hotControl = controlID;
                e.Use(); 
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                GUIUtility.hotControl = 0;
            }
        }

        private void ApplyBrush(GridConfig config, int x, int z)
        {
            GridCellData cellData = config.GetCellData(x, z);
            bool isChanged = false;

            switch (_currentBrushMode)
            {
                case BrushMode.PaintPath:
                    if (cellData.type != NodeType.Path) { cellData.type = NodeType.Path; isChanged = true; }
                    break;
                case BrushMode.PaintGround:
                    if (cellData.type != NodeType.Ground) { cellData.type = NodeType.Ground; isChanged = true; }
                    break;
                case BrushMode.PaintObstacle:
                    if (cellData.type != NodeType.Obstacle) { cellData.type = NodeType.Obstacle; isChanged = true; }
                    break;
                case BrushMode.PaintSpawn:
                    if (cellData.type != NodeType.Spawn) { cellData.type = NodeType.Spawn; isChanged = true; }
                    break;
                case BrushMode.PaintBase:
                    if (cellData.type != NodeType.Base) { cellData.type = NodeType.Base; isChanged = true; }
                    break;
                case BrushMode.RaiseElevation:
                    if (Event.current.type == EventType.MouseDown) { cellData.elevation += 1; isChanged = true; }
                    break;
                case BrushMode.LowerElevation:
                    if (Event.current.type == EventType.MouseDown) { cellData.elevation = Mathf.Max(0, cellData.elevation - 1); isChanged = true; }
                    break;
            }

            if (isChanged)
            {
                config.SetCellData(x, z, cellData);
                EditorUtility.SetDirty(config); 
                SceneView.RepaintAll();
            }
        }
        
        private int controlID => GUIUtility.GetControlID(FocusType.Passive);
    }
}