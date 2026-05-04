using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightMovementRule : BaseMovementRule
{

    protected override void CalculateMoves(Unit unit, Battlefield battlefield, Vector2Int currentPos, List<Cell> availableCells)
    {
        Vector2Int[] knightOffsets = new Vector2Int[]
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

        foreach (var offset in knightOffsets)
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
