using Infrastructure.Signals;
using UnityEngine;

namespace Gameplay.Enemies.FSM
{
    public class ReachedBaseState : EnemyStateBase
    {
        public override EnemyStateType StateType => EnemyStateType.ReachedBase;

        public ReachedBaseState(EnemyFacade facade) : base(facade) { }

        public override void Enter()
        {
            // Отключаем физику, мы уже внутри базы
            Collider col = Facade.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Сигнал, что враг прорвался (база сама отнимет здоровье через OnTriggerEnter, 
            // но стейт должен корректно завершить жизнь врага)
            Facade.ForceDespawn();
        }
    }
}