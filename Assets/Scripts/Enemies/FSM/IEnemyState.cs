using System;
using System.Collections.Generic;

namespace Gameplay.Enemies.FSM
{
    // Перечисление для удобной подписки визуала
    public enum EnemyStateType
    {
        Spawn,
        Move,
        Stunned,
        Death,
        ReachedBase
    }

    public interface IEnemyState
    {
        EnemyStateType StateType { get; }
        void Enter();
        void Tick(float deltaTime);
        void Exit();
    }

}