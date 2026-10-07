using System;
using UnityEngine;

namespace Gameplay.Enemies.FSM
{
    public abstract class EnemyStateBase : IEnemyState
    {
        protected readonly Transform Transform;
        protected readonly Action RequestDespawn;

        public abstract EnemyStateType StateType { get; }

        protected EnemyStateBase(Transform transform, Action requestDespawn = null)
        {
            Transform = transform;
            RequestDespawn = requestDespawn;
        }

        public virtual void Enter() { }
        public virtual void Tick(float deltaTime) { }
        public virtual void Exit() { }
    }
}