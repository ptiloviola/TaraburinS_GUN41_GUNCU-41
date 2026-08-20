using UnityEngine;
using Zenject;
using System.Collections.Generic;
using TpsShooter.Items.Configs;

namespace TpsShooter.Environment
{
    public class LevelLootManager : MonoBehaviour
    {
        [SerializeField] private LevelLootConfig _levelConfig;
        
        [Tooltip("Перетащи сюда пустышки со сцены, где может появиться лут")]
        [SerializeField] private Transform[] _spawnPoints; 

        private LootFactory _factory;

        [Inject]
        public void Construct(LootFactory factory)
        {
            _factory = factory;
        }

        private void Start()
        {
            SpawnLevelLoot();
        }

        private void SpawnLevelLoot()
        {
            if (_levelConfig == null || _spawnPoints.Length == 0) return;

            List<Transform> availablePoints = new List<Transform>(_spawnPoints);
            Shuffle(availablePoints);

            int pointIndex = 0;

            foreach (var request in _levelConfig.LootToSpawn)
            {
                for (int i = 0; i < request.Amount; i++)
                {
                    if (pointIndex >= availablePoints.Count) 
                    {
                        Debug.LogWarning("[LevelLootManager] Не хватает точек спавна для всего лута из конфига!");
                        return;
                    }

                    Transform spawnPoint = availablePoints[pointIndex];
                    _factory.SpawnLoot(request.ItemToSpawn, spawnPoint.position, spawnPoint.rotation);
                    
                    pointIndex++;
                }
            }
        }

        private void Shuffle(List<Transform> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}