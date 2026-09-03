using UnityEngine;
using Gameplay.Core;
using Gameplay.Projectiles.Contracts;
using Gameplay.Projectiles.Payloads;

namespace Gameplay.Projectiles.Data
{
    [CreateAssetMenu(fileName = "AoEPayloadConfig", menuName = "TD/Projectiles/AoE Payload")]
    public class AoEPayloadConfig : PayloadConfig
    {
        [Header("Настройки AoE")]
        public float Radius = 3f;
        public LayerMask EnemyMask;

        public override IProjectilePayload CreatePayload(DamagePayload payload)
        {
            return new AoEPayload(payload, Radius, EnemyMask);
        }
    }
}