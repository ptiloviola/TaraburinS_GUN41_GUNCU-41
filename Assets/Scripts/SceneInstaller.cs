using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    [SerializeField]
    private Battlefield _battlefield;
    [SerializeField, Space(15f)]
    private CellPaletteSettings _cellPaletteSettings;

    private Unit _selectedUnit;
    private List<Cell> _availableMoves = new List<Cell>();


    public override void InstallBindings()
    {

        Container.BindInstance(_battlefield).AsSingle();
        Container.BindInstance(_cellPaletteSettings).AsSingle();

        _battlefield.OnCellClicked += CellManagerOnCellClicked;

        Controls controls = new Controls();
        controls.Enable();
        Container.Bind<Controls>().FromInstance(controls).AsSingle();
        Container.Bind<SceneController>().AsSingle();
        
    }

    private void CellManagerOnCellClicked(Cell clickedCell)
    {
        // clickedCell.SetSelect(_cellPaletteSettings.SelectCell); 

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
        if (_selectedUnit.CurrentCell != null)
        {
            _selectedUnit.CurrentCell.SetSelect(_cellPaletteSettings.SelectCell);
        }
        Debug.Log($"<color=green>Selected Unit: {_selectedUnit.Team}</color>");

        _availableMoves = _selectedUnit.CalculateAvailableMoves(_battlefield);

        Debug.Log($"<color=green>Available Moves: {string.Join(", ", _availableMoves)}</color>");
        foreach (var cell in _availableMoves)
        {
            cell.SetSelect(_cellPaletteSettings.MoveCell);
        }

    }

    //????
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
            if (_selectedUnit.CurrentCell != null)
            {
                _selectedUnit.CurrentCell.ResetSelect();
            }
            _selectedUnit = null;
        }
        foreach (var cell in _availableMoves)
        {
            cell.ResetSelect();
        }
        _availableMoves.Clear();
    }

    private void OnDestroy()
    {
        if (_battlefield != null)
        {
            _battlefield.OnCellClicked -= CellManagerOnCellClicked;
        }
    }



}
