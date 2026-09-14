using System;
using UnityEngine;
using Gameplay.Core.Statuses;

namespace Gameplay.Core.Statuses.Data
{
    [Serializable]
    public class PoisonStatusConfig : IStatusConfig
    {
        [SerializeField] private float _duration = 5f;
        [SerializeField] private float _tickRate = 1f;
        [SerializeField] private int _damagePerTick = 10;

        public IStatusEffect CreateEffect()
        {
            return new PoisonStatus(_duration, _tickRate, _damagePerTick);
        }
    }
}