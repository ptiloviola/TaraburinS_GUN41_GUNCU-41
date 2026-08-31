using Infrastructure.Signals;
using UnityEngine;

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
            }
            
            Collider col = Facade.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // БЕЗ МАГИИ: Берем награду строго из конфига. 
            // Если Config == null, игра выдаст NullReferenceException, и мы сразу поймем, что сломалась инициализация.
            int reward = Facade.Config.Stats.RewardMoney;
            
            Facade.SignalBus.Fire(new SignalEnemyKilled { Reward = reward });

            Facade.ForceDespawn();
        }
    }
}