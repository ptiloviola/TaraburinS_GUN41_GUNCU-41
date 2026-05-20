using UnityEngine;
using Zenject;
using Unity.AI.Navigation; // Подключаем пространство имен нового пакета навигации

namespace Gameplay.Grid
{
    public class GridGenerator : MonoBehaviour
    {
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
        private GridConfig _config; // Ссылка на наш конфиг из папки Settings

        // Внедрение зависимости через метод-конструктор
        [Inject]
        public void Construct(IGridService gridService, GridConfig gridConfig)
        {
            _gridService = gridService;
            _config = gridConfig;
        }

        private void Start()
        {

            // В игре используем конфиг из Zenject. Если его нет, подстрахуемся эдиторским
            GridConfig activeConfig = _config != null ? _config : editorConfig;

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
                    elevationMap[x, z] = _config.GetElevation(x, z);
                    typeMap[x, z] = NodeType.Ground; // Пока все ячейки — земля
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
            for (int x = 0; x < _gridService.Width; x++)
            {
                for (int z = 0; z < _gridService.Height; z++)
                {
                    // Запрашиваем данные ячейки у сервиса
                    GridNode node = _gridService.GetNode(new Vector2Int(x, z));

                    // Рассчитываем позицию куба в пространстве Unity.
                    // Высота (Y) напрямую зависит от Elevation из модели данных
                    float posY = node.Elevation * elevationStep;
                    Vector3 spawnPosition = new Vector3(x * spacing, posY, z * spacing);

                    // Спавним куб
                    GameObject block = Instantiate(cubePrefab, spawnPosition, Quaternion.identity, transform);
                    block.name = $"Node_[{x},{z}]_Height_{node.Elevation}";
                    
                    // Немного растянем куб по вертикали, чтобы получился сплошной рельеф, а не летающие панели
                    Vector3 currentScale = block.transform.localScale;
                    block.transform.localScale = new Vector3(currentScale.x, 0.2f + posY, currentScale.z);
                }
            }
        }

        private void OnDrawGizmos()
        {
            // Рисуем гизмосы только в режиме редактирования и если назначен editorConfig
            if (Application.isPlaying || editorConfig == null) return;

            Gizmos.color = new Color(0f, 1f, 1f, 0.4f); // Полупрозрачный голубой цвет

            for (int x = 0; x < editorConfig.width; x++)
            {
                for (int z = 0; z < editorConfig.height; z++)
                {
                    int elevation = editorConfig.GetElevation(x, z);
                    float posY = elevation * elevationStep;
                    
                    // Вычисляем правильный центр и размер с учетом вытягивания куба вниз к нулю
                    Vector3 center = new Vector3(x * spacing, posY / 2f, z * spacing);
                    Vector3 size = new Vector3(0.9f, 0.2f + posY, 0.9f);

                    Gizmos.DrawCube(center, size);
                    Gizmos.DrawWireCube(center, size);
                }
            }
        }
    }
}
