namespace Gameplay.Towers
{
    public interface ITowerBehavior
    {
        void Initialize(TowerFacade facade);
        void Tick(float deltaTime);

        void Cleanup();
    }

    public interface IBehaviorAdapter
    {
        ITowerBehavior CreateBehavior();
    }
}