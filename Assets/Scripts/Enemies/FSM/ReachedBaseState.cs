using UnityEngine;

namespace Gameplay.Enemies.FSM
{
    public class ReachedBaseState : EnemyStateBase
    {
        public override EnemyStateType StateType => EnemyStateType.ReachedBase;

        public ReachedBaseState(EnemyFacade facade) : base(facade) { }

        public override void Enter()
        {
            Collider col = Facade.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            Facade.ForceDespawn();
        }
    }
}