using System;
using UnityEngine;

namespace Gameplay.Core.Statuses.Data
{
    [Serializable]
    public class FreezeStatusConfig : IStatusConfig
    {
        [SerializeField] private float _baseDuration = 3f;
        [SerializeField] [Range(0f, 1f)] private float _slowPercent = 1f; // 1f = полная заморозка

        public IStatusEffect CreateEffect()
        {
            // Здесь мы создаем тот самый класс FreezeStatus, который ты написал!
            return new FreezeStatus(_baseDuration, _slowPercent);
        }
    }
}