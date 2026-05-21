using UnityEngine;
using Zenject;
using Unity.AI.Navigation; // Подключаем пространство имен нового пакета навигации

namespace Gameplay.Grid
{
    public class GridGenerator : MonoBehaviour
    {
        [Header("Визуальное оформление")]
        [SerializeField] private GridTheme theme;

        [Header("Настройки визуала")]
        [SerializeField] private GameObject cubePrefab; // Прераб серого куба
        [SerializeField] private float spacing = 1.1f;    // Расстояние между кубами
        [SerializeField] private float elevationStep = 0.5f; // Высота одного уровня рельефа

        [Header("Навигация")]
        // Ссылка на компонент, который будет запекать сетку на лету
        [SerializeField] private NavMeshSurface navMeshSurface;

        // ДОБАВИЛИ: Ссылка на конфиг только для отображения в эдиторе
        [Header("Настройка в Редакторе (Gizmos)")]
        [SerializeField] private GridConfig editorConfig;


        private IGridService _gridService;


        // Публичные свойства только для чтения, чтобы наш Editor-скрипт мог брать эти данные
        public float Spacing => spacing;
        public GridConfig EditorConfig => editorConfig;

        // Внедрение зависимости через метод-конструктор
        [Inject]
        public void Construct(IGridService gridService)
        {
            _gridService = gridService;

        }

        private void Start()
        {

            // Берем ТОЛЬКО тот конфиг, который настроен в Инспекторе
            GridConfig activeConfig = editorConfig;

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
            _gridService.InitializeGrid(w, h, elevationMap, typeMap);

            // 2. Строим 3D-мир на основе этих данных
            CreateVisualGrid();

            // 3. КРИТИЧЕСКИЙ ШАГ: Запекаем навигацию прямо в Runtime!
            if (navMeshSurface != null)
            {
                navMeshSurface.BuildNavMesh(); // Движок посмотрит на созданные кубы и построит дороги
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
                    float addedHeight = node.Elevation * elevationStep;
                    // Спавним центр на половине добавленной высоты
                    Vector3 spawnPosition = new Vector3(x * spacing, addedHeight / 2f, z * spacing);

                    // Спавним куб
                    GameObject block = Instantiate(cubePrefab, spawnPosition, Quaternion.identity, transform);
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

                    if (node.Type == NodeType.Path)
                    {
                        modifier.area = pathAreaIndex; // Назначаем зону CustomPath
                        
                        if (theme != null && theme.pathMaterial != null)
                            blockRenderer.material = theme.pathMaterial;
                    }
                    else if (node.Type == NodeType.Obstacle)
                    {
                        modifier.area = 1; // 1 — это встроенная зона Not Walkable (Ходить нельзя никому)
                        
                        if (theme != null && theme.obstacleMaterial != null)
                            blockRenderer.material = theme.obstacleMaterial;
                    }
                    else // NodeType.Ground
                    {
                        modifier.area = groundAreaIndex; // Назначаем зону CustomGround
                        
                        if (theme != null && theme.groundMaterial != null)
                            blockRenderer.material = theme.groundMaterial;
                    }

                }
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying || editorConfig == null) return;

            for (int x = 0; x < editorConfig.width; x++)
            {
                for (int z = 0; z < editorConfig.height; z++)
                {
                    GridCellData cellData = editorConfig.GetCellData(x, z);
                    
                    // 1. Цвета кисточки
                    switch (cellData.type)
                    {
                        case NodeType.Path: 
                            Gizmos.color = new Color(1f, 0.9f, 0f, 0.5f); // Желтый
                            break; 
                        case NodeType.Obstacle: 
                            Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // Красный
                            break; 
                        default: 
                            Gizmos.color = new Color(0f, 1f, 1f, 0.4f); // Голубой
                            break; 
                    }
                    
                    // Считаем, сколько высоты мы добавили ячейке
                    float addedHeight = cellData.elevation * elevationStep;
                    
                    // Центр поднимается ровно на ПОЛОВИНУ добавленной высоты
                    Vector3 center = new Vector3(x * spacing, addedHeight / 2f, z * spacing);
                    
                    // Общий размер (базовая толщина 0.2 + добавленная высота)
                    Vector3 size = new Vector3(0.9f, 0.2f + addedHeight, 0.9f);

                    Gizmos.DrawCube(center, size);
                    Gizmos.DrawWireCube(center, size);
                }
            }
        }
    }
}
