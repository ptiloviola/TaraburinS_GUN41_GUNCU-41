using Gameplay.Auras.Factories;

namespace Gameplay.Projectiles.Contracts
{
    public interface IRequireAuraFactory
    {
        void SetFactory(AuraZoneFactory factory);
    }
}