using UnityEngine;
using Zenject;
using Unity.AI.Navigation; // Подключаем пространство имен нового пакета навигации
using Gameplay.Base;
using Gameplay.Spawning;


namespace Gameplay.Grid
{
    public class GridGenerator : MonoBehaviour
    {
        [Header("Визуальное оформление")]
        [SerializeField] private GridTheme _theme;

        [Header("Настройки визуала")]
        [SerializeField] private GameObject _cubePrefab; // Прераб серого куба
        [SerializeField] private float _spacing = 1.1f;    // Расстояние между кубами
        [SerializeField] private float _elevationStep = 0.5f; // Высота одного уровня рельефа

        [Header("Навигация")]
        // Ссылка на компонент, который будет запекать сетку на лету
        [SerializeField] private NavMeshSurface _navMeshSurface;

        // ДОБАВИЛИ: Ссылка на конфиг только для отображения в эдиторе
        [Header("Настройка в Редакторе (Gizmos)")]
        [SerializeField] private GridConfig _editorConfig;


        private IGridService _gridService;
        private BaseCore.Factory _baseFactory; // НОВОЕ: Внедряем фабрику баз
        private EnemySpawnPoint.Factory _spawnFactory; // НОВОЕ: Фабрика спавнов


        // Публичные свойства только для чтения, чтобы наш Editor-скрипт мог брать эти данные
        public float Spacing => _spacing;
        public GridConfig EditorConfig => _editorConfig;

        // Внедрение зависимости через метод-конструктор
        [Inject]
        public void Construct(IGridService gridService, BaseCore.Factory baseFactory, EnemySpawnPoint.Factory spawnFactory)
        {
            _gridService = gridService;
            _baseFactory = baseFactory; // Получаем фабрику баз от Zenject
            _spawnFactory = spawnFactory;

        }

        private void Start()
        {

            // Берем ТОЛЬКО тот конфиг, который настроен в Инспекторе
            GridConfig activeConfig = _editorConfig;

            if (activeConfig == null)
            {
                Debug.LogError("[GridGenerator] Нет конфигурации сетки!");
                return;
            }

            // Переносим размеры из конфига
            int w = activeConfig.width;
            int h = activeConfig.height;

            int[,] elevationMap = new int[w, h];

            // Карта типов ячеек. Пока пусть вся карта будет обычной землей (Ground)
            NodeType[,] typeMap = new NodeType[w, h];
            // Заполняем матрицы данными из ScriptableObject
            for (int x = 0; x < w; x++)
            {
                for (int z = 0; z < h; z++)
                {
                    GridCellData cellData = activeConfig.GetCellData(x, z);
                    elevationMap[x, z] = cellData.elevation;
                    typeMap[x, z] = cellData.type;
                }
            }

            // 1. Инициализируем математические данные через сервис
            _gridService.InitializeGrid(w, h, elevationMap, typeMap, _spacing, _elevationStep);

            // 2. Строим 3D-мир на основе этих данных
            CreateVisualGrid();

            // 3. КРИТИЧЕСКИЙ ШАГ: Запекаем навигацию прямо в Runtime!
            if (_navMeshSurface != null)
            {
                _navMeshSurface.BuildNavMesh(); // Движок посмотрит на созданные кубы и построит дороги
                Debug.Log("<color=magenta>[GridGenerator] NavMesh успешно запечен в Runtime!</color>");
            }
            else
            {
                Debug.LogError("[GridGenerator] Не назначена ссылка на NavMeshSurface!");
            }
        }

        private void CreateVisualGrid()
        {
            // Получаем индексы зон из настроек Unity по их именам
            int pathAreaIndex = UnityEngine.AI.NavMesh.GetAreaFromName("CustomPath");
            int groundAreaIndex = UnityEngine.AI.NavMesh.GetAreaFromName("CustomGround");

            // БРОНЯ: Проверяем, нашел ли движок наши зоны
            if (pathAreaIndex == -1 || groundAreaIndex == -1)
            {
                Debug.LogError("<color=red>[GridGenerator] ОШИБКА: Зоны CustomPath или CustomGround не найдены! Проверь Window -> AI -> Navigation -> Areas.</color>");
                pathAreaIndex = 0; // Фолбэк на дефолтную зону
                groundAreaIndex = 0;
            }

            for (int x = 0; x < _gridService.Width; x++)
            {
                for (int z = 0; z < _gridService.Height; z++)
                {
                    // Запрашиваем данные ячейки у сервиса
                    GridNode node = _gridService.GetNode(new Vector2Int(x, z));
                    
                    // Рассчитываем позицию куба в пространстве Unity.
                    // Считаем добавленную высоту
                    float addedHeight = node.Elevation * _elevationStep;
                    // Спавним центр на половине добавленной высоты
                    Vector3 spawnPosition = new Vector3(x * _spacing, addedHeight / 2f, z * _spacing);

                    // Спавним куб
                    GameObject block = Instantiate(_cubePrefab, spawnPosition, Quaternion.identity, transform);
                    block.name = $"Node_[{x},{z}]_Height_{node.Elevation}";
                    
                    // Немного растянем куб по вертикали, чтобы получился сплошной рельеф, а не летающие панели
                    Vector3 currentScale = block.transform.localScale;
                    // Растягиваем куб (базовая 0.2 + добавленная высота)
                    block.transform.localScale = new Vector3(currentScale.x, 0.2f + addedHeight, currentScale.z);

                    // --- РАБОТА С МАТЕРИАЛАМИ ---
                    Renderer blockRenderer = block.GetComponent<Renderer>();

                    // --- НАСТРОЙКА НАВИГАЦИОННЫХ ЗОН ---
                    NavMeshModifier modifier = block.AddComponent<NavMeshModifier>();
                    modifier.overrideArea = true;

                    // ПЕРЕД настройкой материалов кубика добавляем логику спавна префаба базы:
                    if (node.Type == NodeType.Base)
                    {
                        // 1. Просим фабрику создать физический префаб базы со всеми инъекциями
                        BaseCore baseInstance = _baseFactory.Create();
                        // 2. Рассчитываем идеальные координаты поверхности куба ячейки
                        Vector3 surfacePos = _gridService.GetWorldPosition(node);
                        // ИСПРАВЛЕНИЕ УТОПЛЕННОСТИ: Прибавляем индивидуальный оффсет префаба базы
                        surfacePos.y += baseInstance.VerticalOffset;
                        // 3. Ставим базу на ее законное место
                        baseInstance.transform.position = surfacePos;
                        // Аккуратно группируем под генератором
                        baseInstance.transform.SetParent(transform);
                        // НОВОЕ: Присваиваем уникальный ID по координатам сетки!
                        baseInstance.BaseId = $"Base_{x}_{z}";
                        // НОВОЕ: Переименовываем GameObject на сцене для удобства геймдизайнера!
                        baseInstance.gameObject.name = $"[MARKER] Base_ID: Base_{x}_{z}";
                    }
                    else if (node.Type == NodeType.Spawn)
                    {
                        // НОВОЕ: Спавним маркер врагов
                        EnemySpawnPoint spawnInstance = _spawnFactory.Create();
                        spawnInstance.transform.position = _gridService.GetWorldPosition(node);
                        spawnInstance.transform.SetParent(transform);
                        
                        // НОВОЕ: Присваиваем уникальный ID по координатам сетки!
                        spawnInstance.PointId = $"Spawn_{x}_{z}";
                        // НОВОЕ: Переименовываем GameObject на сцене для удобства геймдизайнера!
                        spawnInstance.gameObject.name = $"[MARKER] Spawn_ID: Spawn_{x}_{z}";
                    }

                    if (node.Type == NodeType.Path || node.Type == NodeType.Spawn || node.Type == NodeType.Base)
                    {
                        // Спавн и База тоже должны быть проходимыми для врагов!
                        modifier.area = pathAreaIndex; // Назначаем зону CustomPath

                        if(_theme != null)
                        {
                            Material mat = _theme.GetMaterial(node.Type);
                            if (mat != null)
                            {
                                blockRenderer.material = mat;
                            }
                        }
                        
                    }
                    else if (node.Type == NodeType.Obstacle)
                    {
                        modifier.area = 1; // 1 — это встроенная зона Not Walkable (Ходить нельзя никому)
                        
                        if (_theme != null && _theme.obstacleMaterial != null)
                            blockRenderer.material = _theme.obstacleMaterial;
                    }
                    else // NodeType.Ground
                    {
                        modifier.area = groundAreaIndex; // Назначаем зону CustomGround
                        
                        if (_theme != null && _theme.groundMaterial != null)
                            blockRenderer.material = _theme.groundMaterial;
                    }

                }
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying || _editorConfig == null) return;

            for (int x = 0; x < _editorConfig.width; x++)
            {
                for (int z = 0; z < _editorConfig.height; z++)
                {
                    GridCellData cellData = _editorConfig.GetCellData(x, z);
                    
                    // 1. Цвета кисточки
                    switch (cellData.type)
                    {
                        case NodeType.Path: 
                            Gizmos.color = new Color(1f, 0.9f, 0f, 0.5f); // Желтый
                            break; 
                        case NodeType.Obstacle: 
                            Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // Красный
                            break; 
                        case NodeType.Spawn: // НОВОЕ
                            Gizmos.color = new Color(1f, 0f, 1f, 0.6f); // Пурпурный (Маджента)
                            break;
                        case NodeType.Base:  // НОВОЕ
                            Gizmos.color = new Color(0f, 0f, 1f, 0.6f); // Темно-синий
                            break;
                        default: 
                            Gizmos.color = new Color(0f, 1f, 1f, 0.4f); // Голубой (Ground)
                        break;
                    }
                    
                    // Считаем, сколько высоты мы добавили ячейке
                    float addedHeight = cellData.elevation * _elevationStep;
                    
                    // Центр поднимается ровно на ПОЛОВИНУ добавленной высоты
                    Vector3 center = new Vector3(x * _spacing, addedHeight / 2f, z * _spacing);
                    
                    // Общий размер (базовая толщина 0.2 + добавленная высота)
                    Vector3 size = new Vector3(0.9f, 0.2f + addedHeight, 0.9f);

                    Gizmos.DrawCube(center, size);
                    Gizmos.DrawWireCube(center, size);
                    // НОВОЕ: Рисуем парящий текст над Спавном и Базой прямо в редакторе!
#if UNITY_EDITOR
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.white;
                    style.fontStyle = FontStyle.Bold;
                    style.alignment = TextAnchor.MiddleCenter;

                    if (cellData.type == NodeType.Spawn)
                    {
                        UnityEditor.Handles.Label(center + Vector3.up, $"Spawn_{x}_{z}", style);
                    }
                    else if (cellData.type == NodeType.Base)
                    {
                        UnityEditor.Handles.Label(center + Vector3.up, $"Base_{x}_{z}", style);
                    }
#endif
                }
            }
        }
    }
}
