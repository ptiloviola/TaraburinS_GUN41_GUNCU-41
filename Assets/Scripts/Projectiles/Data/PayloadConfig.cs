using UnityEngine;
using Gameplay.Projectiles.Contracts;
using Gameplay.Projectiles.Payloads;

namespace Gameplay.Towers.Data.Payloads
{
    // Это базовый чертеж для любой боевой части
    public abstract class PayloadConfig : ScriptableObject
    {
        // Метод, который создает реальную "посылку" на основе базового урона башни
        public abstract IProjectilePayload CreatePayload(float baseDamage);
    }
}