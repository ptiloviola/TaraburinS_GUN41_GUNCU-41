using System.Collections.Generic;

public class SingleSharedData : ISharedData
{
    public bool Lock { get; set; }
    public GameEvent Event { get; set; }
    public GameStatus Status { get; set; }
    public Unit Destination { get; set; }
    public Cell Target { get; set; }

    public List<Cell> AvailableMoves { get; set; } = new List<Cell>();
}
