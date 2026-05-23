namespace Gameplay.Towers
{
    public interface ITowerBehavior
    {
        void Initialize(TowerFacade facade);
        void Tick(); 
    }
}