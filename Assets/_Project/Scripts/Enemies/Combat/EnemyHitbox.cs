using UnityEngine;
using TpsShooter.Combat;
using TpsShooter.Enemies.Core;

namespace TpsShooter.Enemies.Combat
{
    public class EnemyHitbox : MonoBehaviour, IDamageable
    {
        [SerializeField] private EnemyBrain _brain;
        
        [Tooltip("Множитель урона")]
        [SerializeField] private float _damageMultiplier = 1.0f;
        
        [Tooltip("Опционально: Имя части тела для дебага (Head, Torso, Limbs)")]
        [SerializeField] private string _bodyPartName = "Body";

        public void TakeDamage(float amount)
        {
            if (_brain == null || _brain.Health.IsDead) return;

            float finalDamage = amount * _damageMultiplier;
            
            DevLogger.Log($"<color=orange>[Hitbox]</color> Попадание в {_bodyPartName}! Урон: {amount} x {_damageMultiplier} = {finalDamage}");
            
            _brain.TakeDamage(finalDamage);
        }
    }
}