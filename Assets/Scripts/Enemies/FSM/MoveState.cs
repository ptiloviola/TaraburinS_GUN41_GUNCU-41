using UnityEngine;
using Gameplay.Enemies.Movement;

namespace Gameplay.Enemies.FSM
{
    public class MoveState : EnemyStateBase
    {
        private readonly IMovementStrategy _movementStrategy;

        public override EnemyStateType StateType => EnemyStateType.Move;

        public MoveState(Transform transform, IMovementStrategy movementStrategy) : base(transform)
        {
            _movementStrategy = movementStrategy;
        }

        public override void Enter() { }

        public override void Tick(float deltaTime)
        {
            _movementStrategy?.Tick(deltaTime);
        }
    }
}