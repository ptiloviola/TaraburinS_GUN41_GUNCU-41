using UnityEngine;
using UnityEditor;
using Gameplay.Grid;

namespace Gameplay.Editor
{
    [CustomEditor(typeof(GridGenerator))]
    public class GridGeneratorEditor : UnityEditor.Editor
    {
        // 1. Режимы нашей кисточки
        public enum BrushMode { PaintGround, PaintPath, PaintObstacle, 
        PaintSpawn, PaintBase, RaiseElevation, LowerElevation }
        private BrushMode _currentBrushMode = BrushMode.PaintPath;

        // 2. Рисуем интерфейс прямо в Инспекторе Unity
        public override void OnInspectorGUI()
        {
            // Отрисовка стандартных полей (чтобы spacing и префабы остались видны)
            DrawDefaultInspector();

            GridGenerator generator = (GridGenerator)target;
            GridConfig config = generator.EditorConfig;

            EditorGUILayout.Space(10);
            
            // --- НОВАЯ КНОПКА ИНИЦИАЛИЗАЦИИ ---
            if (GUILayout.Button("🛠 Создать / Сбросить матрицу сетки", GUILayout.Height(30)))
            {
                if (config != null)
                {
                    // Создаем новый массив нужного размера
                    config.rows = new GridRow[config.width];
                    for (int x = 0; x < config.width; x++)
                    {
                        config.rows[x].columns = new GridCellData[config.height];
                        for (int z = 0; z < config.height; z++)
                        {
                            // Заполняем дефолтными значениями (Плоская Земля)
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
            // КНОПКИ КИСТОЧЕК (Сгруппированы по логике)
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(_currentBrushMode == BrushMode.PaintPath, "Дорога (Желтая)", "Button")) _currentBrushMode = BrushMode.PaintPath;
            if (GUILayout.Toggle(_currentBrushMode == BrushMode.PaintGround, "Земля (Голубая)", "Button")) _currentBrushMode = BrushMode.PaintGround;
            if (GUILayout.Toggle(_currentBrushMode == BrushMode.PaintObstacle, "Препятствие (Красное)", "Button")) _currentBrushMode = BrushMode.PaintObstacle;
            GUILayout.EndHorizontal();
            // НОВЫЕ КНОПКИ ДЛЯ СПАВНА И БАЗЫ
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(_currentBrushMode == BrushMode.PaintSpawn, "Спавн (Пурпур)", "Button")) _currentBrushMode = BrushMode.PaintSpawn;
            if (GUILayout.Toggle(_currentBrushMode == BrushMode.PaintBase, "База (Синий)", "Button")) _currentBrushMode = BrushMode.PaintBase;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(_currentBrushMode == BrushMode.RaiseElevation, "Поднять (+)", "Button")) _currentBrushMode = BrushMode.RaiseElevation;
            if (GUILayout.Toggle(_currentBrushMode == BrushMode.LowerElevation, "Опустить (-)", "Button")) _currentBrushMode = BrushMode.LowerElevation;
            GUILayout.EndHorizontal();
            
            EditorGUILayout.HelpBox("Выделите GridGenerator и рисуйте по сетке в окне Scene. ЛКМ = Рисовать.", MessageType.Info);
        }

        private void OnSceneGUI()
        {
            GridGenerator generator = (GridGenerator)target;
            GridConfig config = generator.EditorConfig;

            if (config == null || config.rows == null || config.rows.Length == 0) return;

            Event e = Event.current;

            // 3. Разрешаем рисовать кликом (MouseDown) ИЛИ зажатой кнопкой мыши (MouseDrag)
            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
            {
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                if (groundPlane.Raycast(ray, out float enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    float halfSpacing = generator.Spacing / 2f;
                    
                    int x = Mathf.FloorToInt((hitPoint.x + halfSpacing) / generator.Spacing);
                    int z = Mathf.FloorToInt((hitPoint.z + halfSpacing) / generator.Spacing);

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
                // Отпускаем контроль мыши
                GUIUtility.hotControl = 0;
            }
        }

        private void ApplyBrush(GridConfig config, int x, int z)
        {
            // Используем безопасный метод чтения
            GridCellData cellData = config.GetCellData(x, z);
            bool isChanged = false;

            // Применяем логику в зависимости от выбранной кисти
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
                // НОВЫЕ РЕЖИМЫ КИСТИ:
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

            // Если мы действительно что-то перекрасили/изменили
            if (isChanged)
            {
                // Используем безопасный метод записи! IDE больше ругаться не будет.
                config.SetCellData(x, z, cellData);
                
                EditorUtility.SetDirty(config); 
                SceneView.RepaintAll();
            }
        }
        
        private int controlID => GUIUtility.GetControlID(FocusType.Passive);
    }
}