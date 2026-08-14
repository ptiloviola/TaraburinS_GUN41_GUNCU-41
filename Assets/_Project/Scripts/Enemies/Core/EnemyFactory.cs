using UnityEngine;
using Zenject;
using TpsShooter.Enemies.Configs;

namespace TpsShooter.Enemies.Core
{

    public class EnemyFactory : IEnemyFactory
    {
        private readonly DiContainer _container;

        public EnemyFactory(DiContainer container)
        {
            _container = container;
        }

        public EnemyBrain Create(GameObject prefab, EnemyConfig config, Vector3 position, Quaternion rotation, Transform[] patrolPoints)
        {
            EnemyBrain enemy = _container.InstantiatePrefabForComponent<EnemyBrain>(prefab, position, rotation, null);
            
            // Передаем точки в инициализацию
            enemy.Initialize(config, patrolPoints);
            
            return enemy;
        }
    }
}