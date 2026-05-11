using UnityEngine;
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
            Vector2Int startPos = _data.ActiveUnit.CurrentCell.GridPosition;
            Unit movingUnit = _data.ActiveUnit;
            Cell targetCell = cell;


            CastlingMove(cell);
            Debug.Log($"<color=yellow>[ChessCommand] Ход разрешен! Юнит перемещается на {cell.GridPosition}</color>");
            
            if (cell.Unit != null)
            {
                GameObject.Destroy(cell.Unit.gameObject);
                Debug.Log($"<color=red>[ChessCommand] {cell.Unit.name} УНИЧТОЖЕН!</color>");

            }

            else if (movingUnit.PieceType == PieceType.Pawn && cell.GridPosition.x != startPos.x)
            {
                int backDirection = (movingUnit.Team == Team.White) ? -1 : 1;
                Cell victimCell = _battlefield.GetCell(cell.GridPosition.x, cell.GridPosition.y + backDirection);
                if (victimCell.Unit != null)
                {
                    Debug.Log("<color=red>ВЗЯТИЕ НА ПРОХОДЕ! </color>");
                    GameObject.Destroy(victimCell.Unit.gameObject);
                    victimCell.Unit = null;
                }
            }

            // _data.ActiveUnit.Move(cell);


            movingUnit.CurrentCell.Unit = null;
            targetCell.Unit = movingUnit;
            movingUnit.CurrentCell = targetCell;
            _data.ActiveUnit.HasMoved = true;

            if (_data.ActiveUnit.PieceType == PieceType.Pawn)
            {
                int targetY = cell.GridPosition.y;
                if (targetY == 0 || targetY == 7)
                {
                    _data.ActiveUnit.PromoteToQueen();
                }
            }

            if (_data.ActiveUnit.PieceType == PieceType.Pawn && Mathf.Abs(cell.GridPosition.y - startPos.y) == 2)
            {
                _battlefield.EnPassantTarget = _data.ActiveUnit;
            }
            else
            {
                _battlefield.EnPassantTarget = null;
            }

            
            

            _data.Lock = true;
            ClearSelectionVisuals();
            _data.Status = GameStatus.Select;
            _data.ActiveUnit = null;
            _data.Target = null;
            _data.AvailableMoves.Clear();
            
            movingUnit.Move(targetCell, () => 
            {
                if (movingUnit.PieceType == PieceType.Pawn && targetCell.GridPosition.y == 7)
                {
                    movingUnit.PromoteToQueen();
                }
                _signal.Fire(GameStatus.Confirm);
            });

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

    private void CastlingMove(Cell cell)
    {
        if (_data.ActiveUnit.PieceType == PieceType.King)
        {
            int moveDistance = Mathf.Abs(cell.GridPosition.x - _data.ActiveUnit.CurrentCell.GridPosition.x);
            if (moveDistance == 2)
            {
                bool isShortCastling = (cell.GridPosition.x == 6);
                int rookStartX = isShortCastling ? 7 : 0;
                int rookEndX = isShortCastling ? 5 : 3;
                
                Cell rookStartCell = _battlefield.GetCell(rookStartX, cell.GridPosition.y);
                Cell rookEndCell = _battlefield.GetCell(rookEndX, cell.GridPosition.y);

                if (rookStartCell.Unit != null)
                {
                    Unit rook = rookStartCell.Unit;
                    rookStartCell.Unit = null;
                    rookEndCell.Unit = rook;
                    rook.CurrentCell = rookEndCell;

                    rook.Move(rookEndCell);
                    rook.HasMoved = true;
                    Debug.Log($"<color=cyan>[ChessCommand] Выполнена рокировка! Ладья прыгнула на {rookEndCell.GridPosition}</color>");
                    
                }
            }
            
        }
    }
}
