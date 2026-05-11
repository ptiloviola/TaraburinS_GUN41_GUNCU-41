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

        if (!unit.HasMoved)
        {
            CheckCastling(unit, battlefield, currentPos, 1, availableCells);
            CheckCastling(unit, battlefield, currentPos, -1, availableCells);
        }
        
    }

    private void CheckCastling(Unit king, Battlefield battlefield, Vector2Int kingPos, int direction, List<Cell> availableCells)
    {
        int rookX = (direction > 0) ? 7 : 0;
        Cell rookCell = battlefield.GetCell(rookX, kingPos.y);
        if (rookCell.Unit != null && rookCell.Unit.PieceType == PieceType.Rook && !rookCell.Unit.HasMoved && rookCell.Unit.Team == king.Team)
        {
            if (IsPathClear(kingPos.x, rookX, kingPos.y, battlefield))
            {
                int targetX = (direction > 0) ? 6 : 2;
                availableCells.Add(battlefield.GetCell(targetX, kingPos.y));

            }
            
        }

    }

    private bool IsPathClear(int startX, int endX, int y, Battlefield battlefield)
    {
        int min = Mathf.Min(startX, endX);
        int max = Mathf.Max(startX, endX);

        for (int x = min + 1; x < max; x++)
        {
            Cell cell = battlefield.GetCell(x, y);
            if (!IsCellEmpty(cell))
            {
                return false;
            }
        }
        return true; 
    }

}
