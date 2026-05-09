
using System.Collections.Generic;

public class ChessRuleValidator
{
    private readonly Battlefield _battlefield;

    public ChessRuleValidator(Battlefield battlefield)
    {
        _battlefield = battlefield;
    }

    public bool IsKingOnCheck(Team team)
    {
        Cell kingCell = FindKingCell(team);
        if (kingCell == null)
        {
            return false;
        }
        return IsCellUnderAttack(kingCell, team);
    }

    private Cell FindKingCell(Team team)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Cell cell = _battlefield.GetCell(x, y);
                if (cell.Unit != null && cell.Unit.Team == team && cell.Unit.PieceType == PieceType.King)
                {
                    return cell;
                }
            }
        }
        return null;
    }

    private bool IsCellUnderAttack(Cell targetCell, Team defendefTeam)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Cell cell = _battlefield.GetCell(x, y);
                if (cell != null && cell.Unit != null && cell.Unit.Team != defendefTeam)
                {
                    Unit enemyUnit = cell.Unit;
                    IMovementRule enemyRule = GetRuleForUnit(enemyUnit);
                    if (enemyRule != null)
                    {
                        List<Cell> enemyMoves = enemyRule.GetAvailableMoves(enemyUnit, _battlefield);
                        if (enemyMoves.Contains(targetCell))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private IMovementRule GetRuleForUnit(Unit unit) 
    {
        return unit.MovementRule;
    }

    public List<Cell> GetLegalMoves(Unit unit)
    {
        List<Cell> legalMoves = new List<Cell>();
        IMovementRule movementRule = GetRuleForUnit(unit);
        List<Cell> rawMoves = movementRule.GetAvailableMoves(unit, _battlefield);
        Cell startCell = unit.CurrentCell;
        foreach(Cell targetCell in rawMoves)
        {
            Unit victim = targetCell.Unit;
            startCell.Unit = null;
            targetCell.Unit = unit;
            unit.CurrentCell = targetCell;

            bool isCheck = IsKingOnCheck(unit.Team);

            targetCell.Unit = victim;
            startCell.Unit = unit;
            unit.CurrentCell = startCell;

            if (!isCheck)
            {
                legalMoves.Add(targetCell);
            }  
        }
        return legalMoves;
    }



}
