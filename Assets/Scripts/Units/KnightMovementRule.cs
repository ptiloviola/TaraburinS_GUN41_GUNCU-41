using System.Collections.Generic;
using UnityEngine;

public class KnightMovementRule : BaseMovementRule
{
    private readonly Vector2Int[] _knightOffsets = new Vector2Int[]
        {
            new Vector2Int(1, 2),
            new Vector2Int(2, 1),
            new Vector2Int(2, -1),
            new Vector2Int(1, -2),
            new Vector2Int(-1, -2),
            new Vector2Int(-2, -1),
            new Vector2Int(-2, 1),
            new Vector2Int(-1, 2),
        };

    protected override void CalculateMoves(Unit unit, Battlefield battlefield, Vector2Int currentPos, List<Cell> availableCells)
    {
        foreach (var offset in _knightOffsets)
        {
            Vector2Int targetPos = currentPos + offset;
            Cell targetCell = battlefield.GetCell(targetPos.x, targetPos.y);

            if (targetCell != null)
            {
                if (IsCellEmpty(targetCell) || IsEnemyOnCell(unit, targetCell))
                {
                    availableCells.Add(targetCell);
                }
            }
        }
    }

}
