using System;
using UnityEngine;

public class CellManager : MonoBehaviour
{
    private Cell[] _cells;
    public event Action<Cell> OnCellClicked;

    [SerializeField, Header("grid settings")]
    private float _neighbourSearchRadius = 3.0f;

    private void Awake()
    {
        InitializeBoard();
        LinkUnitsToCells();

        Debug.Log($"<color=cyan>CellManager initialized! Cells found: {_cells.Length}</color>");
    }


    private void InitializeBoard()
    {
        _cells = FindObjectsOfType<Cell>();
        foreach (var cell in _cells)
        {
            cell.OnPointerClickEvent += OnCellClicked;
            FindNeighbours(cell);
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
        Debug.Log($"<color=cyan>[CellManager] Units found on scene: {allUnits.Length}</color>");
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

            Debug.Log($"<color=white>[CellManager] Unit {unit} is closest to {nearestCell}. Distance: {minDistance}</color>");
            if (nearestCell != null && minDistance < 2.0f)
            {
                unit.CurrentCell = nearestCell;
                nearestCell.Unit = unit;
                Vector3 snapPosition = nearestCell.transform.position;
                snapPosition.y = unit.transform.position.y;
                unit.transform.position = snapPosition;
                Debug.Log($"<color=green>[CellManager] Connection Established: {unit} <-> {nearestCell}</color>");
            }
            else
            {
                Debug.Log($"<color=red>[CellManager] CONNECTION ERROR: {unit} is too far from {nearestCell}, or the cell was not found!</color>");
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var cell in _cells)
        {
            cell.OnPointerClickEvent -= OnCellClicked;
        }
    }

}
