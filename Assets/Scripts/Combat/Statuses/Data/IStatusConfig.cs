namespace Gameplay.Combat.Statuses.Data
{
    public interface IStatusConfig
    {
        IStatusEffect CreateEffect();
    }
}