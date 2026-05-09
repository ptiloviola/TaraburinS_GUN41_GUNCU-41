using UnityEngine;
using UnityEngine.XR;
using Zenject;

public class ChessCommand : IGameplayCommand
{
    private ISharedData _data;
    private SignalBus _signal;

    private Battlefield _battlefield;

    private ITurn _turn;

    private ChessRuleValidator _chessRuleValidator;

    [Inject]
    public void Construct(ISharedData data, SignalBus signal, Battlefield battlefield, ITurn turn, ChessRuleValidator chessRuleValidator)
    {
        _data = data;
        _signal = signal;
        _battlefield = battlefield;
        _turn = turn;
        _chessRuleValidator = chessRuleValidator;
        Debug.Log("<color=magenta>[ChessCommand] Создан и получил зависимости </color>");
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

            case GameStatus.Attack:
            HandleAttack(cell);
            break;
        }
    }

    private void HandleSelection(Cell cell)
    {
        if (cell.Unit != null)
        {
            if (cell.Unit != null)
            {
                if (cell.Unit.Team != _turn.Current)
                {
                    Debug.Log($"<color=orange>[ChessCommand] Сейчас ход {_turn.Current}, выбрана фигура команды {cell.Unit.Team}</color>");
                    return;
                }
            }
            Debug.Log($"<color=white>[ChessCommand] На клетке найден юнит {cell.Unit.name}. Записываю в SharedData...</color>");
            _data.ActiveUnit = cell.Unit;
            _data.Target = cell;
            _data.Status = GameStatus.Move;

            _data.ActiveUnit.SetHighlight(true);
            _battlefield.HighlightSelectedCell(_data.Target);

            // _data.AvailableMoves = _data.ActiveUnit.CalculateAvailableMoves(_battlefield);
            _data.AvailableMoves = _chessRuleValidator.GetLegalMoves(_data.ActiveUnit);

            _battlefield.HighlightAvailableMoves(_data.AvailableMoves, _data.ActiveUnit);

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
            
            if (cell.Unit != null)
            {
                GameObject.Destroy(cell.Unit.gameObject);
                Debug.Log($"<color=red>[ChessCommand] {cell.Unit.name} УНИЧТОЖЕН!</color>");

            }
            _data.ActiveUnit.Move(cell);
            ClearSelectionVisuals();
            _data.Status = GameStatus.Select;
            _data.ActiveUnit = null;
            _data.Target = null;
            _data.AvailableMoves.Clear();

            _signal.Fire(GameStatus.Confirm);
        }
        else
        {
            Debug.Log("<color=red>[ChessCommand] Клик мимо хода. Сброс выделения.</color>");
            ClearSelectionVisuals();
            _data.Status = GameStatus.Select;
            _data.ActiveUnit = null;
            _data.Target = null;
            _data.AvailableMoves.Clear();

            _signal.Fire(GameEvent.Cancel);
        }
    }

    private void HandleAttack(Cell cell)
    {
        Debug.Log($"<color=white>[ChessCommand] Режим Attack. Можно просто убивать {cell.GridPosition}.</color>");
        if (cell.Unit != null)
        {
            GameObject.Destroy(cell.Unit.gameObject);
            _data.Status = GameStatus.Select;
            Debug.Log($"<color=red>[CHEAT] Цель {cell.Unit.name} уничтожена смертельным лучом </color>");
            _data.AvailableMoves.Clear();
        }
        else
        {
            Debug.Log("<color=gray>[CHEAT] Промах. Тут нет юнита.</color>");
            _data.Status = GameStatus.Select;
        }
    }

    private void ClearSelectionVisuals()
    {
        if (_data.ActiveUnit != null)
        {
            _data.ActiveUnit.SetHighlight(false);
        }
        _battlefield.ClearHighlighting(_data.Target, _data.AvailableMoves);
    }
}
