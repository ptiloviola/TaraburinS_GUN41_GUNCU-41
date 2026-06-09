using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Spawning.Data
{
    [Serializable]
    public class WaveData
    {
        [Tooltip("Время на подготовку ПЕРЕД началом этой волны")]
        public float DelayBeforeWave = 5.0f;
        
        [Tooltip("Отряды, из которых состоит эта волна")]
        public List<SquadData> Squads = new List<SquadData>();
        
        [Tooltip("Награда за полную зачистку волны")]
        public int ClearReward = 50;
    }
}
