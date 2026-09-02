using Infrastructure.Signals;
using UnityEngine;
using Cysharp.Threading.Tasks; // Требуется UniTask
using Gameplay.Enemies.Visuals;

namespace Gameplay.Enemies.FSM
{
    public class DeathState : EnemyStateBase
    {
        public override EnemyStateType StateType => EnemyStateType.Death;

        public DeathState(EnemyFacade facade) : base(facade) { }

        public override void Enter()
        {
            // Останавливаем физику и движение
            if (Facade.Agent != null && Facade.Agent.isActiveAndEnabled)
            {
                Facade.Agent.isStopped = true;
                Facade.Agent.enabled = false; // Лучше вообще выключить агента, чтобы его не толкали
            }
            
            Collider col = Facade.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            int reward = Facade.Config.Stats.RewardMoney;
            Facade.SignalBus.Fire(new SignalEnemyKilled { Reward = reward });

            if (Facade.Config.DeathBehavior != null)
            {
                Facade.Config.DeathBehavior.Execute(Facade);
            }

            // Запускаем асинхронный процесс без блокировки основного потока
            ProcessDeathAsync().Forget();
        }

        private async UniTaskVoid ProcessDeathAsync()
        {
            // 1. Ищем визуализатор
            var visuals = Facade.GetComponent<EnemyVisualsBase>();
            if (visuals != null)
            {
                // 2. Ждем, пока проиграется красивая анимация смерти
                await visuals.PlayDeathAnimationAsync();
            }

            // 3. Только после этого убираем труп в пул
            Facade.ForceDespawn();
        }
    }
}