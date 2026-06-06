namespace Gameplay.Towers.Data.Modules
{
    // Интерфейс - это контракт. Любой класс, который его "подписывает", 
    // обязан иметь метод GetStatsDescription.
    public interface IModuleDescriptor
    {
        string GetStatsDescription();
    }
}