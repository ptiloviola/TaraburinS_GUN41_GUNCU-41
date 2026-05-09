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

    protected void AddMovesInDirection(Unit unit, Battlefield battlefield, Vector2Int startPos, Vector2Int direction, List<Cell> availableCells, int lengthMove = 8)
    {

        Vector2Int currentPos = startPos;
        int currentStep = 0;
        while(currentStep < lengthMove)
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
            currentStep += 1;
        }

    }

    public static Vector2Int[] GetAllDirection()
    {
        List<Vector2Int> queenDirection = new List<Vector2Int>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }
                queenDirection.Add(new Vector2Int(x, y));
            }
        }
        return queenDirection.ToArray();
    }

}
