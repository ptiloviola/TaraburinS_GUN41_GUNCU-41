namespace Gameplay.Combat.Statuses.Data
{
    public interface IStatusConfig
    {
        // Фабричный метод: конфиг сам знает, какой статус он должен породить
        IStatusEffect CreateEffect();
    }
}