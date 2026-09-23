namespace Gameplay.Interaction
{
    public enum InteractionMode
    {
        Normal,
        TacticalClaim,
        Building
    }

    public class InteractionStateModel
    {
        public InteractionMode CurrentMode { get; set; } = InteractionMode.Normal;
        public int AvailableClaims { get; set; } = 0;
    }
}