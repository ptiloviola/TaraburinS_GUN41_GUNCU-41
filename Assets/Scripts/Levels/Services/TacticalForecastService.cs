using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Gameplay.Spawning;

namespace Gameplay.Levels.Services
{
    public class ObfuscatedEnemyData
    {
        public string EnemyId;        
        public bool IsTypeHidden;     
        public int TotalCount;        
        public bool IsCountHidden;    
    }

    public class TacticalForecastService
    {

        [System.Serializable]
        public class Settings
        {
            [Range(0f, 1f)] public float HideTypeChance = 0.25f;
            [Range(0f, 1f)] public float HideCountChance = 0.35f;
        }

        private readonly IWaveProvider _waveProvider;
        private readonly Settings _settings;

        public TacticalForecastService(IWaveProvider waveProvider, Settings settings)
        {
            _waveProvider = waveProvider;
            _settings = settings;
        }

        public List<ObfuscatedEnemyData> GetLevelForecast()
        {
            var allWaves = _waveProvider.GetAllWaves();
            var totals = new Dictionary<string, int>();

            foreach (var wave in allWaves)
            {
                foreach (var squad in wave.Squads)
                {
                    if (totals.ContainsKey(squad.EnemyId))
                        totals[squad.EnemyId] += squad.Count;
                    else
                        totals[squad.EnemyId] = squad.Count;
                }
            }

            var forecast = new List<ObfuscatedEnemyData>();
            foreach (var kvp in totals)
            {

                bool hideType = Random.value < _settings.HideTypeChance;
                bool hideCount = Random.value < _settings.HideCountChance;

                forecast.Add(new ObfuscatedEnemyData
                {
                    EnemyId = kvp.Key,
                    TotalCount = kvp.Value,
                    IsTypeHidden = hideType,
                    IsCountHidden = hideCount
                });
            }

            return forecast.OrderBy(x => Random.value).ToList();
        }
    }
}