using TpsShooter.Enemies.Configs;
using TpsShooter.Player;

namespace TpsShooter.Enemies.Core
{
    // Единый интерфейс для любого типа атаки (оружие, когти, зубы, магия)
    public interface IEnemyCombatHandler
    {
        void Initialize(EnemyConfig config);
        void PerformAttack(PlayerFacade target, EnemyAnimator animator);
        void OnDeath();
    }
}