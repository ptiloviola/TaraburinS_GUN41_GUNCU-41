using UnityEngine;

namespace VacuumSim.Pathfinding
{
    [RequireComponent(typeof(BoxCollider))]
    public class PathfindingGrid : MonoBehaviour
    {
        [Header("Размеры и настройки")]
        [Tooltip("Коллайдер, определяющий границы комнаты (Сетка построится по его размерам)")]
        [SerializeField] private BoxCollider _gridBoundsCollider;
        
        [Tooltip("Радиус одной ячейки (должен быть чуть больше радиуса робота)")]
        [SerializeField] private float _nodeRadius;

        [Header("Маски физики")]
        [Tooltip("Слои, по которым ездить НЕЛЬЗЯ (Стены, Мебель, Мусор)")]
        [SerializeField] private LayerMask _unwalkableMask;

        private Node[,] _grid;
        private float _nodeDiameter;
        private int _gridSizeX;
        private int _gridSizeY;
        
        private Vector2 _gridWorldSize; 

        public System.Collections.Generic.List<Node> CurrentPath;

        public int GridSizeX => _gridSizeX;
        public int GridSizeY => _gridSizeY;

        private void Reset()
        {
            _gridBoundsCollider = GetComponent<BoxCollider>();
            if (_gridBoundsCollider != null) _gridBoundsCollider.isTrigger = true;
        }

        private void Awake()
        {
            if (_gridBoundsCollider == null)
            {
                Debug.LogError("[Grid] НЕ НАЗНАЧЕН КОЛЛАЙДЕР ГРАНИЦ СЕТКИ!");
                return;
            }

            _nodeDiameter = _nodeRadius * 2;
            
            _gridWorldSize = new Vector2(_gridBoundsCollider.bounds.size.x, _gridBoundsCollider.bounds.size.z);
            
            _gridSizeX = Mathf.RoundToInt(_gridWorldSize.x / _nodeDiameter);
            _gridSizeY = Mathf.RoundToInt(_gridWorldSize.y / _nodeDiameter);
            
            CreateGrid();
        }

        public void CreateGrid()
        {
            _grid = new Node[_gridSizeX, _gridSizeY];
            
            Vector3 worldBottomLeft = new Vector3(
                _gridBoundsCollider.bounds.center.x - _gridWorldSize.x / 2,
                transform.position.y,
                _gridBoundsCollider.bounds.center.z - _gridWorldSize.y / 2
            );

            _gridBoundsCollider.enabled = false;

            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft 
                        + Vector3.right * (x * _nodeDiameter + _nodeRadius) 
                        + Vector3.forward * (y * _nodeDiameter + _nodeRadius);

                    bool isWalkable = !Physics.CheckBox(
                        worldPoint, 
                        new Vector3(_nodeRadius, 0.5f, _nodeRadius), 
                        Quaternion.identity, 
                        _unwalkableMask
                    );

                    _grid[x, y] = new Node(isWalkable, worldPoint, x, y);
                }
            }
            
            _gridBoundsCollider.enabled = true;
            
            Debug.Log($"[Grid] Сетка сгенерирована: {_gridSizeX * _gridSizeY} ячеек. Размер: {_gridWorldSize.x} x {_gridWorldSize.y}");
        }

        public Node NodeFromWorldPoint(Vector3 worldPosition)
        {
            if (_gridBoundsCollider == null) return null;

            float percentX = (worldPosition.x - _gridBoundsCollider.bounds.min.x) / _gridWorldSize.x;
            float percentY = (worldPosition.z - _gridBoundsCollider.bounds.min.z) / _gridWorldSize.y;
            
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((_gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((_gridSizeY - 1) * percentY);

            return _grid[x, y];
        }

        public System.Collections.Generic.List<Node> GetNeighbors(Node node)
        {
            var neighbors = new System.Collections.Generic.List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    int checkX = node.GridX + x;
                    int checkY = node.GridY + y;

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
        }
        
        private void OnDrawGizmos()
        {
            if (_gridBoundsCollider != null)
            {
                Gizmos.DrawWireCube(_gridBoundsCollider.bounds.center, new Vector3(_gridBoundsCollider.bounds.size.x, 1, _gridBoundsCollider.bounds.size.z));
            }

            if (_grid != null)
            {
                foreach (Node node in _grid)
                {
                    Gizmos.color = node.IsWalkable ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.5f);

                    if (node.IsCleaned)
                    {
                        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.5f); 
                    }
                    
                    if (CurrentPath != null && CurrentPath.Contains(node))
                    {
                        Gizmos.color = Color.black; 
                    }
                    
                    Gizmos.DrawCube(node.WorldPosition, Vector3.one * (_nodeDiameter - 0.05f));
                }
            }
        }

        public float GetDirtyPercentage()
        {
            if (_grid == null || _grid.Length == 0) return 0f;
            
            int walkableCount = 0;
            int trashCount = 0;
            
            foreach (Node node in _grid)
            {
                if (node.IsWalkable)
                {
                    walkableCount++;
                    if (node.HasTrash) trashCount++; 
                }
            }
            
            return walkableCount == 0 ? 0f : (float)trashCount / walkableCount;
        }
    }
}