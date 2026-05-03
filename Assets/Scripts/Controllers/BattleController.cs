
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class BattleController : MonoBehaviour
{
    private Battlefield _battlefield;
    private IGameplayCommand _command;
    private ISharedData _data;
    private SignalBus _signal;
    private Controls.GameActions _controls;

    // private Unit _selectedUnit;

    private List<Cell> _availableMoves = new List<Cell>();

    [Inject]
    private void Construct(Battlefield battlefield, IGameplayCommand command, ISharedData data, SignalBus signal, Controls controls)
    {
        _battlefield = battlefield;
        _command = command;
        _data = data;
        _signal = signal;
        _controls = controls.Game;
        
        // _battlefield.OnCellClicked += HandleCellClick;
        _battlefield.OnCellClicked += _command.Interact;

        _controls.Cancel.performed += OnCancel;
        _controls.Confirm.performed += OnConfirm;

        _signal.Subscribe<GameEvent>(Callback);
        Debug.Log("<color=green>[BattleController] Успешно инициализирован и слушает эфир.</color>");

        _data.Status = GameStatus.Select;

    }

    private void OnCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("<color=orange>[BattleController] Нажат ESC (Cancel). Сброс статуса в Select.</color>");
        _data.Event = GameEvent.Cancel;
        _data.Status = GameStatus.Select;
    }

    private void OnConfirm(InputAction.CallbackContext obj)
    {
        if (_data.Target == null)
        {
            Debug.Log("<color=red>[BattleController] Нажат пробел, но клетка не выбрана!</color>");
            return;
        }
        Debug.Log("<color=yellow>[BattleController] Подтверждение действия!</color>");
        _signal.Fire(GameStatus.Confirm);
    }

    private void Callback(GameEvent arg)
    {
        Debug.Log($"<color=green>[BattleController] УСЛЫШАЛ СИГНАЛ: {arg}. Текущий статус: {_data.Status}</color>");
        if (arg != GameEvent.Select) return;

        switch (_data.Status)
        {
            case GameStatus.Move:
                Debug.Log("<color=white>[BattleController] Статус Move. Ждем, пока игрок выберет клетку для хода.</color>");
                break;

        }
    }

    private void OnDestroy()
    {
        _battlefield.OnCellClicked -= _command.Interact;
        _controls.Cancel.performed -= OnCancel;
        _controls.Confirm.performed -= OnConfirm;
        _signal.Unsubscribe<GameEvent>(Callback);
    }

    // private void HandleCellClick(Cell clickedCell)
    // {
    //     if (clickedCell.Unit != null)
    //     {
    //         SelectUnit(clickedCell.Unit);
    //     }
    //     else
    //     {
    //         if (_selectedUnit != null)
    //         {
    //             if (_availableMoves.Contains(clickedCell))
    //             {
    //                 Debug.Log($"<color=yellow>Unit {_selectedUnit} moves to {clickedCell.GridPosition}</color>");
    //                 ExecuteMove(clickedCell);
    //             }
    //             else
    //             {
    //                 Debug.Log("<color=red>Invalid move! Selection cleared.</color>");
    //                 ClearSelection();
    //             }
    //         }
    //         else
    //         {
    //             Debug.Log("<color=red>You clicked on an empty cell, but no unit is selected!</color>");
    //         }
    //     }
    // }

    // private void SelectUnit(Unit unit)
    // {
    //     ClearSelection();
    //     _selectedUnit = unit;
    //     _selectedUnit.SetHighlight(true);

    //     _battlefield.HighlightSelectedCell(_selectedUnit.CurrentCell);
    //     Debug.Log($"<color=green>Selected Unit: {_selectedUnit.Team}</color>");

    //     _availableMoves = _selectedUnit.CalculateAvailableMoves(_battlefield);
    //     _battlefield.HighlightAvailableMoves(_availableMoves);
    //     Debug.Log($"<color=green>Available Moves: {string.Join(", ", _availableMoves)}</color>");
    // }

    // private void ExecuteMove(Cell targetCell)
    // {
    //     _selectedUnit.Move(targetCell);
    //     ClearSelection();
    // }

    // private void ClearSelection()
    // {
    //     if (_selectedUnit != null)
    //     {
    //         _selectedUnit.SetHighlight(false);

    //         _battlefield.ClearHighlighting(_selectedUnit.CurrentCell, _availableMoves);
    //         _selectedUnit = null;
    //     }
    //     _availableMoves.Clear();
    // }

    // private void OnDestroy()
    // {
    //     if (_battlefield != null)
    //     {
    //         _battlefield.OnCellClicked -= HandleCellClick;
    //     }
    // }


}
