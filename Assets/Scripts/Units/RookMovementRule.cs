using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class RookMovementRule : BaseMovementRule
{
    private readonly Vector2Int[] _rookDirection = new Vector2Int[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, -1)
    };
    protected override void CalculateMoves(Unit unit, Battlefield battlefield, Vector2Int currentPos, List<Cell> availableCells)
    {
        foreach (var direction in _rookDirection)
        {
            AddMovesInDirection(unit, battlefield, currentPos, direction, availableCells);
        }
        
    }
}
