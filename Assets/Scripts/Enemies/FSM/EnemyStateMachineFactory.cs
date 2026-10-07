using UnityEngine.AI;
using UnityEngine;
using Zenject;
using Gameplay.Enemies.Data;
using Gameplay.Enemies.Visuals;
using System;
using Gameplay.Enemies.Movement;

namespace Gameplay.Enemies.FSM
{
    public class EnemyStateMachineFactory
    {
        private readonly SignalBus _signalBus;

        public EnemyStateMachineFactory(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public EnemyStateMachine Create(Transform transform, Action requestDespawn, NavMeshAgent agent, Collider collider, EnemyVisualsBase visuals, EnemyConfig config, IMovementStrategy movement)
        {
            var stateMachine = new EnemyStateMachine();

            stateMachine.AddState(new MoveState(transform, movement));
            stateMachine.AddState(new DeathState(
                transform, requestDespawn, agent, collider, config.DeathBehavior, _signalBus, visuals, config.Stats.RewardMoney));
            stateMachine.AddState(new ReachedBaseState(transform, requestDespawn, collider));

            return stateMachine;
        }
    }
}