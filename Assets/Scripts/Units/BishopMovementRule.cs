using System.Collections.Generic;
using UnityEngine;

public class BishopMovementRule : BaseMovementRule
{
    private readonly Vector2Int[] _bishopDirections = new Vector2Int[]
        {
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1),
        };
    protected override void CalculateMoves(Unit unit, Battlefield battlefield, Vector2Int currentPos, List<Cell> availableCells)
    {
        
        foreach (var direction in _bishopDirections)
        {
            AddMovesInDirection(unit, battlefield, currentPos, direction, availableCells);
        }
    }

}
