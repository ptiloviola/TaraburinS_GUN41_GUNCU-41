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
    Lock = 1,
    Unlock = 2,
    Select = 3,
    Move = 4, 
    Attack = 5, 
    Confirm = 6
}

public enum GameEvent
{
    Empty = 0,
    Select = 1,   
    Cancel = 2,   
    Confirm = 3   
}