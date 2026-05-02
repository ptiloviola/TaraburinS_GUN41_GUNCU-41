using System.Collections.Generic;

public interface IMovementRule
{
    List<Cell> GetAvailableMoves (Unit unit, Battlefield battlefield);
}