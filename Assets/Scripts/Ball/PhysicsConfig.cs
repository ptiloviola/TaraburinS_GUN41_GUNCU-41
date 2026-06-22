using System;
using UnityEngine;

namespace Bowling.Ball
{
    [Serializable] 
    public class PhysicsConfig
    
    {

        [Header("Настройки Add Force")]
        public float AddForceMultiplier = 1.7f;

        [Header("Настройки Linear Velocity")]
        public float VelocityMultiplier = 0.5f;

        [Header("Настройки Move Position")]
        public float MovePositionMultiplier = 0.42f;
        public float FrictionDecay = 0.98f;

    }
}
