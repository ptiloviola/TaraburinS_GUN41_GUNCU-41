using Gameplay.Infrastructure.Signals;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Gameplay.Enemies.Visuals;

namespace Gameplay.Enemies.FSM
{
    public class DeathState : EnemyStateBase
    {
        public override EnemyStateType StateType => EnemyStateType.Death;

        public DeathState(EnemyFacade facade) : base(facade) { }

        public override void Enter()
        {

            if (Facade.Agent != null && Facade.Agent.isActiveAndEnabled)
            {
                Facade.Agent.isStopped = true;
                Facade.Agent.enabled = false; 
            }
            
            Collider col = Facade.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            int reward = Facade.Config.Stats.RewardMoney;
            Facade.SignalBus.Fire(new SignalEnemyKilled { Reward = reward });

            if (Facade.Config.DeathBehavior != null)
            {
                Facade.Config.DeathBehavior.Execute(Facade);
            }

            ProcessDeathAsync().Forget();
        }

        private async UniTaskVoid ProcessDeathAsync()
        {
            var visuals = Facade.GetComponent<EnemyVisualsBase>();
            if (visuals != null)
            {
                await visuals.PlayDeathAnimationAsync();
            }

            Facade.ForceDespawn();
        }
    }
}