using TpsShooter.Enemies.Configs;
using TpsShooter.Player;

namespace TpsShooter.Enemies.Core
{
    public interface IEnemyCombatHandler
    {
        void Initialize(EnemyConfig config);
        void PerformAttack(PlayerFacade target, EnemyAnimator animator);
        void OnDeath();
    }
}