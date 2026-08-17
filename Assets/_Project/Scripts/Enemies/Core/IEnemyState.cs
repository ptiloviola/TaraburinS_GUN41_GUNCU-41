using UnityEngine;

namespace TpsShooter.Enemies.Core
{
    public interface IEnemyState
    {
        Color StateGizmoColor { get; } // Для ТЗ: цвет зависит от состояния
        
        void Enter();
        void Tick();
        void Exit();
        void OnDamageTaken(); // Делегирование реакции на урон стейту
    }
}