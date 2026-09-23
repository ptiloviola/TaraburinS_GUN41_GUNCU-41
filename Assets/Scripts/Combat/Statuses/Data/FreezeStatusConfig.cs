using System;
using UnityEngine;

namespace Gameplay.Combat.Statuses.Data
{
    [Serializable]
    public class FreezeStatusConfig : IStatusConfig
    {
        [SerializeField] private float _baseDuration = 3f;
        [SerializeField] [Range(0f, 1f)] private float _slowPercent = 1f;

        public IStatusEffect CreateEffect()
        {
            return new FreezeStatus(_baseDuration, _slowPercent);
        }
    }
}