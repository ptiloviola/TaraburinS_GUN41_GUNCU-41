using UnityEngine;
using System;

namespace Gameplay.Enemies.FSM
{
    public class ReachedBaseState : EnemyStateBase
    {
        private readonly Collider _collider;

        public override EnemyStateType StateType => EnemyStateType.ReachedBase;

        public ReachedBaseState(Transform transform, Action requestDespawn, Collider collider) : base(transform, requestDespawn)
        {
            _collider = collider;
        }

        public override void Enter()
        {
            if (_collider != null) _collider.enabled = false;
            
            RequestDespawn?.Invoke();
        }
    }
}