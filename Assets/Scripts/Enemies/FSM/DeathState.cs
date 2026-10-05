using Gameplay.Infrastructure.Signals;
using Cysharp.Threading.Tasks;
using Gameplay.Enemies.Visuals;
using Gameplay.Enemies.Data.Death;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Gameplay.Enemies.FSM
{
    public class DeathState : EnemyStateBase
    {
        private readonly NavMeshAgent _agent;
        private readonly Collider _collider;
        private readonly DeathBehaviorConfig _deathBehavior;
        private readonly SignalBus _signalBus;
        private readonly EnemyVisualsBase _visuals;
        private readonly int _rewardMoney;

        public override EnemyStateType StateType => EnemyStateType.Death;

        public DeathState(
            EnemyFacade facade, 
            NavMeshAgent agent, 
            Collider collider, 
            DeathBehaviorConfig deathBehavior, 
            SignalBus signalBus, 
            EnemyVisualsBase visuals, 
            int rewardMoney) : base(facade)
        {
            _agent = agent;
            _collider = collider;
            _deathBehavior = deathBehavior;
            _signalBus = signalBus;
            _visuals = visuals;
            _rewardMoney = rewardMoney;
        }

        public override void Enter()
        {
            if (_agent != null && _agent.isActiveAndEnabled)
            {
                _agent.isStopped = true;
                _agent.enabled = false; 
            }
            
            if (_collider != null) _collider.enabled = false;

            _signalBus.Fire(new SignalEnemyKilled { Reward = _rewardMoney });

            if (_deathBehavior != null)
            {
                _deathBehavior.Execute(Facade, _signalBus);
            }

            ProcessDeathAsync().Forget();
        }

        private async UniTaskVoid ProcessDeathAsync()
        {
            if (_visuals != null)
            {
                await _visuals.PlayDeathAnimationAsync();
            }

            Facade.RequestDespawn();
        }
    }
}