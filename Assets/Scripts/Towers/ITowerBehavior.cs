namespace Gameplay.Towers
{
    public interface ITowerBehavior
    {
        void Initialize(TowerFacade facade);
        void Tick(float deltaTime);
    }

    public interface IBehaviorAdapter
    {
        ITowerBehavior CreateBehavior();
    }
}