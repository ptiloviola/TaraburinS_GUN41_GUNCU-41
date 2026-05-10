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
            if (currentPos.y == 1 || currentPos.y == 6)
            {
                Cell forwardCellExtra = battlefield.GetCell(currentPos.x, currentPos.y + forwardDirection * 2);
                if (IsCellEmpty(forwardCellExtra))
                {
                    availableCells.Add(forwardCellExtra);
                }
            }
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

        if (battlefield.EnPassantTarget != null && battlefield.EnPassantTarget.Team != unit.Team)
        {
            Vector2Int epPos = battlefield.EnPassantTarget.CurrentCell.GridPosition;
            if (epPos.y == currentPos.y && Mathf.Abs(epPos.x - currentPos.x) == 1)
            {
                Cell epCell = battlefield.GetCell(epPos.x, epPos.y + forwardDirection);
                if (IsCellEmpty(epCell)) 
                {
                    availableCells.Add(epCell);
                }
            }
        }
    }
}
    
    
