using UnityEngine;

namespace TpsShooter.Enemies.Core
{
    public interface IEnemyState
    {
        Color StateGizmoColor { get; }
        
        void Enter();
        void Tick();
        void Exit();
        void OnDamageTaken();
    }
}