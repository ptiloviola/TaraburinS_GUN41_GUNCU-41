using UnityEngine;
using UnityEngine.XR;
using Zenject;

public class ChessCommand : IGameplayCommand
{
    private ISharedData _data;
    private SignalBus _signal;

    private Battlefield _battlefield;

    [Inject]
    public void Construct(ISharedData data, SignalBus signal, Battlefield battlefield)
    {
        _data = data;
        _signal = signal;
        _battlefield = battlefield;
        Debug.Log("<color=magenta>[ChessCommand] Создан и получил зависимости (Пульт и Радио)!</color>");
    }
    public void Interact(Cell cell)
    {
        if (_data.Lock) return;
        Debug.Log($"<color=cyan>[ChessCommand] Получил клик по клетке {cell.GridPosition}. Текущий статус в SharedData: {_data.Status}</color>");

        switch (_data.Status)
        {
            case GameStatus.Select:
            HandleSelection(cell);
            break;
            
            case GameStatus.Move:
            HandleMovement(cell);
            break;
        }
    }

    private void HandleSelection(Cell cell)
    {
        if (cell.Unit != null)
        {
            Debug.Log($"<color=white>[ChessCommand] На клетке найден юнит {cell.Unit.name}. Записываю в SharedData...</color>");
            _data.Destination = cell.Unit;
            _data.Target = cell;
            _data.Status = GameStatus.Move;

            _data.Destination.SetHighlight(true);
            _battlefield.HighlightSelectedCell(_data.Target);

            _data.AvailableMoves = _data.Destination.CalculateAvailableMoves(_battlefield);

            _battlefield.HighlightAvailableMoves(_data.AvailableMoves);

            Debug.Log("<color=yellow>[ChessCommand] Статус изменен на Move. Бросаю сигнал GameEvent.Select в эфир!</color>");

            _signal.Fire(GameEvent.Select);
            Debug.Log($"[ChessCommand] Piece selected: {cell.Unit.name}. Transitioning to Move status.");
            
        }
        else
        {
            Debug.Log("<color=gray>[ChessCommand] Клик по пустой клетке в режиме Select. Игнорирую.</color>");
        }
    }

    private void HandleMovement(Cell cell)
    {
        Debug.Log($"<color=white>[ChessCommand] Режим Move. Игрок хочет пойти на клетку {cell.GridPosition}.</color>");
        if (_data.AvailableMoves.Contains(cell))
        {
            Debug.Log($"<color=yellow>[ChessCommand] Ход разрешен! Юнит перемещается на {cell.GridPosition}</color>");
            _data.Destination.Move(cell);
            ClearSelectionVisuals();
            _data.Status = GameStatus.Select;
            _data.Destination = null;
            _data.Target = null;
            _data.AvailableMoves.Clear();
        }
        else
        {
            Debug.Log("<color=red>[ChessCommand] Клик мимо хода. Сброс выделения.</color>");
            ClearSelectionVisuals();
            _data.Status = GameStatus.Select;
            _data.Destination = null;
            _data.Target = null;
            _data.AvailableMoves.Clear();

            _signal.Fire(GameEvent.Cancel);
        }
    }

    private void ClearSelectionVisuals()
    {
        if (_data.Destination != null)
        {
            _data.Destination.SetHighlight(false);
        }
        _battlefield.ClearHighlighting(_data.Target, _data.AvailableMoves);
    }
}
