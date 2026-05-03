using System.Collections.Generic;
public interface ISharedData
{
    bool Lock { get; set; }
    GameEvent Event { get; set; }
    GameStatus Status { get; set; }

    Unit Destination { get; set; }

    Cell Target {get; set; }

    List<Cell> AvailableMoves { get; set; }
}

