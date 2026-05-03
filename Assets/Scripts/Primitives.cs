using System;

public enum NeighbourType
{
    None,
    Forward,
    Backward,
    Left,
    Right,
    ForwardLeft,
    ForwardRight,
    BackwardLeft,
    BackwardRight
}

public enum Team
{
    White,
    Black
}

public enum PieceType 
{ 
    Pawn,
    Rook,
    Knight,
    Bishop,
    Queen,
    King
}


public enum GameStatus
{
    Error = 0,
    Lock = 1,     // Инпут заблокирован (например, идет анимация хода)
    Unlock = 2,   // Инпут разрешен
    Select = 3,   // Игрок выбирает свою фигуру
    Move = 4,     // Игрок выбрал фигуру и кликает, куда пойти
    Attack = 5,   // Игрок выбирает, кого атаковать
    Confirm = 6   // Подтверждение действия (если игра требует нажатия Space)
}

public enum GameEvent
{
    Empty = 0,
    Select = 1,   // Произошел выбор
    Cancel = 2,   // Игрок нажал отмену (ESC)
    Confirm = 3   // Игрок подтвердил действие
}