
using System;
using System.Collections.Generic;
using UnityEngine;

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

            bool isPathBlockedByAttack = IsCastlingPathBlocked(unit, targetCell, startCell, victim);

            bool isCheck = IsKingOnCheck(unit.Team);

            

            targetCell.Unit = victim;
            startCell.Unit = unit;
            unit.CurrentCell = startCell;

            if (!isCheck && !isPathBlockedByAttack)
            {
                legalMoves.Add(targetCell);
            }  
        }
        return legalMoves;
    }

    public bool IsCheckMate(Team team)
    {
        if (!IsKingOnCheck(team))
        {
            return false;
        }
        for (int x = 0; x < 8; x ++)
        {
            for (int y = 0; y < 8; y ++)
            {
                Cell cell = _battlefield.GetCell(x, y);
                if (cell != null && cell.Unit != null && cell.Unit.Team == team)
                {
                    List<Cell> legalMoves = GetLegalMoves(cell.Unit);
                    if (legalMoves.Count > 0)
                    {
                        Debug.Log($"[Validator] Мата нет, потому что {cell.Unit.PieceType} " +
                              $"на {cell.GridPosition} может пойти на {legalMoves[0].GridPosition}");
                        return false;
                    }
                    
                }
            }
        }
        return true;
    }

    private bool IsCastlingPathBlocked(Unit unit, Cell targetCell, Cell startCell, Unit victim)
    {
        if (unit.PieceType == PieceType.King && MathF.Abs(targetCell.GridPosition.x - startCell.GridPosition.x) == 2)
        {
            unit.CurrentCell = startCell;
            targetCell.Unit = victim;
            startCell.Unit = unit;
            bool isCurrentlyInCheck = IsKingOnCheck(unit.Team);

            startCell.Unit = null;
            targetCell.Unit = unit;
            unit.CurrentCell = targetCell;

            if (isCurrentlyInCheck)
            {
                return true;
            }
            else
            {
                int direction = (targetCell.GridPosition.x - startCell.GridPosition.x) > 0 ? 1: -1;
                int middleX = startCell.GridPosition.x + direction;
                Cell middleCell = _battlefield.GetCell(middleX, targetCell.GridPosition.y);
                if (IsCellUnderAttack(middleCell, unit.Team))
                {
                    return true;
                }
            }
        }
        return false;
    }



}
