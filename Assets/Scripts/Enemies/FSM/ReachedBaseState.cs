using UnityEngine;

namespace Gameplay.Enemies.FSM
{
    public class ReachedBaseState : EnemyStateBase
    {
        private readonly Collider _collider;

        public override EnemyStateType StateType => EnemyStateType.ReachedBase;

        public ReachedBaseState(EnemyFacade facade, Collider collider) : base(facade)
        {
            _collider = collider;
        }

        public override void Enter()
        {
            if (_collider != null) _collider.enabled = false;
            
            Facade.RequestDespawn();
        }
    }
}