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

        // Внедрение зависимости через метод-конструктор
        [Inject]
        public void Construct(IGridService gridService)
        {
            _gridService = gridService;
        }

        private void Start()
        {
            // Для MVP Спринта 1 жестко закодируем карту 5х5 с перепадами высот
            // 0 - низина, 1 - возвышенность, 2 - гора
            int[,] elevationMap = new int[5, 5] {
                { 0, 0, 0, 0, 0 },
                { 0, 1, 1, 1, 0 },
                { 0, 1, 2, 1, 0 },
                { 0, 1, 1, 1, 0 },
                { 0, 0, 0, 0, 0 }
            };

            // Карта типов ячеек. Пока пусть вся карта будет обычной землей (Ground)
            NodeType[,] typeMap = new NodeType[5, 5];
            for (int x = 0; x < 5; x++)
            {
                for (int z = 0; z < 5; z++)
                {
                    typeMap[x, z] = NodeType.Ground;
                }
            }

            // 1. Инициализируем математические данные через сервис
            _gridService.InitializeGrid(5, 5, elevationMap, typeMap);

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
