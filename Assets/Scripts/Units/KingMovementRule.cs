using System.Collections.Generic;
using UnityEngine;

public class KingMovementRule : BaseMovementRule
{
    private readonly Vector2Int[] _kingDirection = GetAllDirection();


    protected override void CalculateMoves(Unit unit, Battlefield battlefield, Vector2Int currentPos, List<Cell> availableCells)
    {
        foreach (var direction in _kingDirection)
        {
           AddMovesInDirection(unit, battlefield, currentPos, direction, availableCells, 1);
        }
        
    }
}
