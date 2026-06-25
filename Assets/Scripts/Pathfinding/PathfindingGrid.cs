using UnityEngine;

namespace VacuumSim.Pathfinding
{
    public class PathfindingGrid : MonoBehaviour
    {
        [Header("Размеры и настройки")]
        [Tooltip("Физический размер зоны сканирования (X и Z)")]
        [SerializeField] private Vector2 _gridWorldSize;
        
        [Tooltip("Радиус одной ячейки (должен быть чуть больше радиуса робота)")]
        [SerializeField] private float _nodeRadius;

        [Header("Маски физики")]
        [Tooltip("Слои, по которым ездить НЕЛЬЗЯ (Стены, Мебель, Мусор)")]
        [SerializeField] private LayerMask _unwalkableMask;

        private Node[,] _grid;
        private float _nodeDiameter;
        private int _gridSizeX;
        private int _gridSizeY;

        // Хранилище для визуализации пути
        public System.Collections.Generic.List<Node> CurrentPath;

        public int GridSizeX => _gridSizeX;
        public int GridSizeY => _gridSizeY;

        // Позже Zenject будет вызывать этот метод через интерфейс
        private void Awake()
        {
            _nodeDiameter = _nodeRadius * 2;
            // Вычисляем, сколько ячеек поместится в наши размеры
            _gridSizeX = Mathf.RoundToInt(_gridWorldSize.x / _nodeDiameter);
            _gridSizeY = Mathf.RoundToInt(_gridWorldSize.y / _nodeDiameter);
            
            CreateGrid();
        }

        public void CreateGrid()
        {
            _grid = new Node[_gridSizeX, _gridSizeY];
            
            // Находим левый нижний угол нашей виртуальной сетки
            Vector3 worldBottomLeft = transform.position 
                - Vector3.right * _gridWorldSize.x / 2 
                - Vector3.forward * _gridWorldSize.y / 2;

            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {
                    // Вычисляем центр текущей ячейки
                    Vector3 worldPoint = worldBottomLeft 
                        + Vector3.right * (x * _nodeDiameter + _nodeRadius) 
                        + Vector3.forward * (y * _nodeDiameter + _nodeRadius);

                    // Делаем физическую проверку: свободна ли эта зона?
                    // CheckBox вернет true, если внутри куба есть объект из _unwalkableMask
                    bool isWalkable = !Physics.CheckBox(
                        worldPoint, 
                        new Vector3(_nodeRadius, 0.5f, _nodeRadius), // Вытянутый вверх куб проверок
                        Quaternion.identity, 
                        _unwalkableMask
                    );

                    _grid[x, y] = new Node(isWalkable, worldPoint, x, y);
                }
            }
            
            Debug.Log($"[Grid] Сетка сгенерирована: {_gridSizeX * _gridSizeY} ячеек.");
        }

        // Переводит 3D координаты (например, позицию пылесоса) в узел сетки
        public Node NodeFromWorldPoint(Vector3 worldPosition)
        {
            // Находим позицию относительно центра сетки в процентах (от 0 до 1)
            float percentX = (worldPosition.x + _gridWorldSize.x / 2) / _gridWorldSize.x;
            float percentY = (worldPosition.z + _gridWorldSize.y / 2) / _gridWorldSize.y;
            
            // Защита от выхода за пределы (зажимаем значения)
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            // Получаем индексы массива
            int x = Mathf.RoundToInt((_gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((_gridSizeY - 1) * percentY);

            return _grid[x, y];
        }

        // Возвращает список всех соседей ячейки (полезно для шагов алгоритма)
        public System.Collections.Generic.List<Node> GetNeighbors(Node node)
        {
            var neighbors = new System.Collections.Generic.List<Node>();

            // Перебираем матрицу 3х3 вокруг текущего узла
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    // Пропускаем саму себя
                    if (x == 0 && y == 0) continue;

                    int checkX = node.GridX + x;
                    int checkY = node.GridY + y;

                    // Если сосед не выходит за границы карты - добавляем в список
                    if (checkX >= 0 && checkX < _gridSizeX && checkY >= 0 && checkY < _gridSizeY)
                    {
                        neighbors.Add(_grid[checkX, checkY]);
                    }
                }
            }
            return neighbors;
        }

        public Node GetNodeFromIndices(int x, int y)
        {
            return _grid[x, y];
        }

        public void ResetCleaningMemory()
        {
            if (_grid == null) return;
            foreach (Node node in _grid)
            {
                node.IsCleaned = false;
            }
            Debug.Log("[Grid] Память об уборке стерта. Сетка снова считается грязной.");
        }
        
        // ==========================================
        // МАГИЯ ОТРИСОВКИ ДЛЯ РАЗРАБОТЧИКА
        // ==========================================
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(_gridWorldSize.x, 1, _gridWorldSize.y));

            if (_grid != null)
            {
                foreach (Node node in _grid)
                {
                    Gizmos.color = node.IsWalkable ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.5f);

                    // Если ячейка УЖЕ убрана — красим её в красивый полупрозрачный синий или желтый
                    if (node.IsCleaned)
                    {
                        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.5f); // Желтый след уборки
                    }
                    
                    // ИЗМЕНЕНИЕ: Если ячейка есть в нашем пути, красим ее в черный!
                    if (CurrentPath != null && CurrentPath.Contains(node))
                    {
                        Gizmos.color = Color.black; 
                    }
                    
                    Gizmos.DrawCube(node.WorldPosition, Vector3.one * (_nodeDiameter - 0.05f));
                }
            }
        }


    }
}