using System.Collections.Generic;
using UnityEngine;


public class PawnMovementRule : BaseMovementRule
{
    protected override void CalculateMoves(Unit unit, Battlefield battlefield, Vector2Int currentPos, List<Cell> availableCells)
    {
        int forwardDirection = (unit.Team == Team.White) ? 1 : -1;
        Cell forwardCell = battlefield.GetCell(currentPos.x, currentPos.y + forwardDirection);
        if (IsCellEmpty(forwardCell)) 
        {
            availableCells.Add(forwardCell);
        }

        Cell attackLeft = battlefield.GetCell(currentPos.x - 1, currentPos.y + forwardDirection);
        if (IsEnemyOnCell(unit, attackLeft))
        {
            availableCells.Add(attackLeft);
        }
        Cell attackRight = battlefield.GetCell(currentPos.x + 1, currentPos.y + forwardDirection);
        if (IsEnemyOnCell(unit, attackRight))
        {
            availableCells.Add(attackRight);
        }
    }
}
    
    
