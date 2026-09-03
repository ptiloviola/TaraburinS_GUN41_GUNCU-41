namespace Gameplay.Towers
{
    // Чистая бизнес-логика (живет в памяти)
    public interface ITowerBehavior
    {
        void Initialize(TowerFacade facade);
        void Tick(float deltaTime); // Передаем deltaTime для чистоты
    }

    // Адаптер (висит на GameObject)
    public interface IBehaviorAdapter
    {
        ITowerBehavior CreateBehavior();
    }
}