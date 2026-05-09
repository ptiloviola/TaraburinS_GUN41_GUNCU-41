using System.Collections.Generic;
using UnityEngine;

public class QueenMovementRule : BaseMovementRule
{
    private readonly Vector2Int[] _queenDirection = GetAllDirection();
    
    
    protected override void CalculateMoves(Unit unit, Battlefield battlefield, Vector2Int currentPos, List<Cell> availableCells)
    {
        foreach (var direction in _queenDirection)
        {
            AddMovesInDirection(unit, battlefield, currentPos, direction, availableCells);
        }
    }
}
