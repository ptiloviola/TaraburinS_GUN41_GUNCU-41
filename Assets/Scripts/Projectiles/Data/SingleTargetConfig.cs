using UnityEngine;
using Gameplay.Core; 
using Gameplay.Projectiles.Contracts;
using Gameplay.Projectiles.Payloads;

namespace Gameplay.Projectiles.Data
{
    [CreateAssetMenu(fileName = "SingleTargetConfig", menuName = "TD/Projectiles/Single Target Payload")]
    public class SingleTargetConfig : PayloadConfig
    {
        public override IProjectilePayload CreatePayload(DamagePayload payload)
        {
            return new SingleTargetPayload(payload);
        }
    }
}