using UnityEngine;
using Gameplay.Combat; 
using Gameplay.Projectiles.Contracts;

namespace Gameplay.Projectiles.Data
{
    public abstract class PayloadConfig : ScriptableObject
    {
        public abstract IProjectilePayload CreatePayload(DamagePayload payload);
    }
}