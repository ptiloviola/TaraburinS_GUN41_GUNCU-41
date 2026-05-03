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
    Select,
    Move,
    Attack,
    Confirm,
}

public enum GameEvent
{
    Cancel,
    Confirm,
    Select,
}