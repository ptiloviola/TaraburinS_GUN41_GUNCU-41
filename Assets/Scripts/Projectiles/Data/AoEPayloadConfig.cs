using UnityEngine;
using Gameplay.Projectiles.Contracts;
using Gameplay.Projectiles.Payloads;

namespace Gameplay.Towers.Data.Payloads
{
    [CreateAssetMenu(fileName = "AoEConfig", menuName = "TD/Payloads/Area of Effect (Splash)")]
    public class AoEPayloadConfig : PayloadConfig
    {
        [Header("Настройки взрыва")]
        public float ExplosionRadius = 3f;
        public LayerMask EnemyMask; // Укажи здесь слой "Enemy"

        public override IProjectilePayload CreatePayload(float baseDamage)
        {
            return new AoEPayload(baseDamage, ExplosionRadius, EnemyMask);
        }
    }
}