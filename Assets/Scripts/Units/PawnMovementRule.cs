using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Rendering;

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
    }
}
    
    
