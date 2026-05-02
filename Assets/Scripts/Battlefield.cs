using System;
using UnityEngine;

public class Battlefield : MonoBehaviour
{
    private Cell[] _cells;

    private Cell[,] _grid = new Cell[8, 8];
    public event Action<Cell> OnCellClicked;

    [SerializeField, Header("grid settings")]
    private float _neighbourSearchRadius = 3.0f;

    private void Awake()
    {
        InitializeBoard();
        LinkUnitsToCells();

        Debug.Log($"<color=cyan>Battlefield initialized! Board 8x8 successfully mapped.</color>");
    }


    private void InitializeBoard()
    {
        _cells = FindObjectsOfType<Cell>();
        // foreach (var cell in _cells)
        // {
        //     cell.OnPointerClickEvent += OnCellClicked;
        //     FindNeighbours(cell);
        // }
        float minX = float.MaxValue;
        float minZ = float.MaxValue;
        foreach (var cell in _cells)
        {
            if (cell.transform.position.x < minX)
            {
                minX = cell.transform.position.x;
            }
            if (cell.transform.position.z < minZ)
            {
                minZ = cell.transform.position.z;
            }
        }

        foreach (var cell in _cells)
        {
            cell.OnPointerClickEvent += OnCellClicked;

            int gridX = Mathf.RoundToInt((cell.transform.position.x - minX) / 2f);
            int gridY = Mathf.RoundToInt((cell.transform.position.z - minZ) / 2f);
            if ( gridX >= 0 && gridX < 8 && gridY >=0 && gridY < 8)
            {
                _grid[gridX, gridY] = cell;
                cell.GridPosition = new Vector2Int(gridX, gridY);
                cell.gameObject.name = $"Cell [{gridX}, {gridY}]";
            }
            else
            {
                Debug.LogError($"<color=red>Cell out of bounds: {gridX}, {gridY}</color>");
            }
        }

    }

    private void FindNeighbours(Cell targetCell)
    {
        Vector3 source = targetCell.transform.position;

        foreach (var otherCell in _cells)
        {
            if (targetCell == otherCell) continue;

            Vector3 destination = otherCell.transform.position;

            float distance = Vector3.Distance(source, destination);

            if (distance < _neighbourSearchRadius)
            {
                int forward = destination.z.CompareTo(source.z);
                int right = destination.x.CompareTo(source.x);

                NeighbourType type = (forward, right) switch
                {
                    (1, 0) => NeighbourType.Forward,
                    (-1, 0) => NeighbourType.Backward,
                    (0, -1) => NeighbourType.Left,
                    (0, 1) => NeighbourType.Right,
                    (1, -1) => NeighbourType.ForwardLeft,
                    (1, 1) => NeighbourType.ForwardRight,
                    (-1, -1) => NeighbourType.BackwardLeft,
                    (-1, 1) => NeighbourType.BackwardRight,
                    _ => NeighbourType.None
                };
                if (type != NeighbourType.None)
                {
                    targetCell.AddNeighbour(type, otherCell);
                }
            }

        }
    }

    private void LinkUnitsToCells()
    {
        Unit[] allUnits = FindObjectsOfType<Unit>();
        Debug.Log($"<color=cyan>[Battlefield] Units found on scene: {allUnits.Length}</color>");
        foreach(var unit in allUnits)
        {
            Cell nearestCell = null;
            float minDistance = float.MaxValue;

            foreach (var cell in _cells)
            {
                Vector2 unitPos2D = new Vector2(unit.transform.position.x, unit.transform.position.z);
                
                Vector2 cellPos2D = new Vector2(cell.transform.position.x, cell.transform.position.z);
                float distance = Vector2.Distance(unitPos2D, cellPos2D);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestCell = cell;
                }
            }

            Debug.Log($"<color=white>[Battlefield] Unit {unit} is closest to {nearestCell}. Distance: {minDistance}</color>");
            if (nearestCell != null && minDistance < 2.0f)
            {
                unit.CurrentCell = nearestCell;
                nearestCell.Unit = unit;
                Vector3 snapPosition = nearestCell.transform.position;
                snapPosition.y = unit.transform.position.y;
                unit.transform.position = snapPosition;
                Debug.Log($"<color=green>[Battlefield] Connection Established: {unit} <-> {nearestCell}</color>");
            }
            else
            {
                Debug.Log($"<color=red>[Battlefield] CONNECTION ERROR: {unit} is too far from {nearestCell}, or the cell was not found!</color>");
            }
        }
    }

    public Cell GetCell(int x, int y)
    {
        if (x >= 0 && x < 8 && y >= 0 && y < 8)
        {
            return _grid[x, y];
        }
        return null;
    }

    public Cell GetCell(Vector2Int position)
    {
        return GetCell(position.x, position.y);
    }

    private void OnDestroy()
    {
        foreach (var cell in _cells)
        {
            cell.OnPointerClickEvent -= OnCellClicked;
        }
    }

}
