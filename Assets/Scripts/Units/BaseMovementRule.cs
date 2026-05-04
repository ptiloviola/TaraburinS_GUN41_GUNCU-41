using System.Collections.Generic;
using UnityEngine;

public abstract class BaseMovementRule : IMovementRule
{

    public List<Cell> GetAvailableMoves(Unit unit, Battlefield battlefield)
    {
        List<Cell> availableCells = new List<Cell>();
        if (unit.CurrentCell == null) return availableCells;

        Vector2Int currentPos = unit.CurrentCell.GridPosition;

        CalculateMoves(unit, battlefield, currentPos, availableCells);
        return availableCells;
    }

    protected abstract void CalculateMoves(Unit unit, Battlefield battlefield, Vector2Int currentPos, List<Cell> availableCells);

    protected bool IsCellEmpty(Cell cell)
    {
        return cell != null && cell.Unit == null;
    }

    protected bool IsEnemyOnCell(Unit myUnit, Cell cell)
    {
        
        return cell != null && cell.Unit != null && cell.Unit.Team != myUnit.Team;
    }

    protected void AddMovesInDirection(Unit unit, Battlefield battlefield, Vector2Int startPos, Vector2Int direction, List<Cell> availableCells)
    {

        Vector2Int currentPos = startPos;
        while(true)
        {
            Vector2Int targetPos = currentPos + direction;
            Cell targetCell = battlefield.GetCell(targetPos.x, targetPos.y);
            if (targetCell == null)
            {
                break;
            }
            if (IsCellEmpty(targetCell))
            {
                availableCells.Add(targetCell);
                currentPos = targetPos;
            } 
            else if (IsEnemyOnCell(unit, targetCell))
            {
                availableCells.Add(targetCell);
                break;
            }
            else
            {
                break;
            }
        }

    }

    protected void CheckDirection(Unit unit, Battlefield battlefield, Vector2Int startPos, Vector2Int direction, List<Cell> availableCells)
    {
        
    }

}
