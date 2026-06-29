using UnityEngine;
using Gameplay.Projectiles.Contracts;
using Gameplay.Projectiles.Payloads;

namespace Gameplay.Towers.Data.Payloads
{
    [CreateAssetMenu(fileName = "SingleTargetConfig", menuName = "TD/Payloads/Single Target")]
    public class SingleTargetConfig : PayloadConfig
    {
        // В этом конфиге нет настроек, так как одиночный урон зависит только от базового Damage
        public override IProjectilePayload CreatePayload(float baseDamage)
        {
            return new SingleTargetPayload(baseDamage);
        }
    }
}