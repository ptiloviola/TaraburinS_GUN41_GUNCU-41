using UnityEngine;
using Zenject;

namespace Gameplay.Grid
{
    public class GridGenerator : MonoBehaviour
    {
        [Header("Настройки визуала")]
        [SerializeField] private GameObject cubePrefab; // Прераб серого куба
        [SerializeField] private float spacing = 1.1f;    // Расстояние между кубами
        [SerializeField] private float elevationStep = 0.5f; // Высота одного уровня рельефа

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
            // Переносим размеры из конфига
            int w = _config.width;
            int h = _config.height;

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
    }
}
