
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BattleController : MonoBehaviour
{
    private Battlefield _battlefield;

    private Unit _selectedUnit;

    private List<Cell> _availableMoves = new List<Cell>();

    [Inject]
    private void Construct(Battlefield battlefield)
    {
        _battlefield = battlefield;
        
        _battlefield.OnCellClicked += HandleCellClick;
    }

    private void HandleCellClick(Cell clickedCell)
    {
        if (clickedCell.Unit != null)
        {
            SelectUnit(clickedCell.Unit);
        }
        else
        {
            if (_selectedUnit != null)
            {
                if (_availableMoves.Contains(clickedCell))
                {
                    Debug.Log($"<color=yellow>Unit {_selectedUnit} moves to {clickedCell.GridPosition}</color>");
                    ExecuteMove(clickedCell);
                }
                else
                {
                    Debug.Log("<color=red>Invalid move! Selection cleared.</color>");
                    ClearSelection();
                }
            }
            else
            {
                Debug.Log("<color=red>You clicked on an empty cell, but no unit is selected!</color>");
            }
        }
    }

    private void SelectUnit(Unit unit)
    {
        ClearSelection();
        _selectedUnit = unit;
        _selectedUnit.SetHighlight(true);
        
        _battlefield.HighlightSelectedCell(_selectedUnit.CurrentCell);
        Debug.Log($"<color=green>Selected Unit: {_selectedUnit.Team}</color>");

        _availableMoves = _selectedUnit.CalculateAvailableMoves(_battlefield);
        _battlefield.HighlightAvailableMoves(_availableMoves);
        Debug.Log($"<color=green>Available Moves: {string.Join(", ", _availableMoves)}</color>");
    }

    private void ExecuteMove(Cell targetCell)
    {
        _selectedUnit.Move(targetCell);
        ClearSelection();
    }

    private void ClearSelection()
    {
        if (_selectedUnit != null)
        {
            _selectedUnit.SetHighlight(false);

            _battlefield.ClearHighlighting(_selectedUnit.CurrentCell, _availableMoves);
            _selectedUnit = null;
        }
        _availableMoves.Clear();
    }

    private void OnDestroy()
    {
        if (_battlefield != null)
        {
            _battlefield.OnCellClicked -= HandleCellClick;
        }
    }


}
