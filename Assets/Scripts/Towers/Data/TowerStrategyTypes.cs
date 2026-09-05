namespace Gameplay.Towers.Data
{
    public enum TargetingType
    {
        Closest,
        Farthest,
        Strongest
    }

    public enum AimingType
    {
        Omni,       // Вращается во все стороны (ПВО, Магия)
        Horizontal  // Вращается только по оси Y (Пушки, Арбалеты)
    }

    public enum ExecutorType
    {
        Hitscan,
        Projectile
    }
}