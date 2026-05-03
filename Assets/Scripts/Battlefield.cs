using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Battlefield : MonoBehaviour
{
    private Cell[] _cells;

    private Cell[,] _grid = new Cell[8, 8];
    public event Action<Cell> OnCellClicked;

    [SerializeField, Header("grid size")]
    private float _gridSize = 2.0f;

    private void Awake()
    {
        InitializeBoard();
        LinkUnitsToCells();

        Debug.Log($"<color=cyan>Battlefield initialized! Board 8x8 successfully mapped.</color>");
    }

    private CellPaletteSettings _cellPaletteSettings;

    [Inject]
    private void Construct(CellPaletteSettings cellPaletteSettings)
    {
        _cellPaletteSettings = cellPaletteSettings;
    }


    private void InitializeBoard()
    {
        _cells = FindObjectsOfType<Cell>();

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

            int gridX = Mathf.RoundToInt((cell.transform.position.x - minX) / _gridSize);
            int gridY = Mathf.RoundToInt((cell.transform.position.z - minZ) / _gridSize);
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

    public void HighlightSelectedCell(Cell cell)
    {
        if (cell != null)
        {
            cell.SetSelect(_cellPaletteSettings.SelectCell);
        }
    }

    public void HighlightAvailableMoves(List<Cell> cells)
    {
        foreach (var cell in cells)
        {
            cell.SetSelect(_cellPaletteSettings.MoveCell);
        }
    }

    public void ClearHighlighting(Cell selectedCell, List<Cell> availableMoves)
    {
        if (selectedCell != null)
        {
            selectedCell.ResetSelect();
        }
        foreach (var cell in availableMoves)
        {
            cell.ResetSelect();
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
