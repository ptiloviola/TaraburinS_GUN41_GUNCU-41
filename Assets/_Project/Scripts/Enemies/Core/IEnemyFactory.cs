using UnityEngine;
using Zenject;
using TpsShooter.Enemies.Configs;

namespace TpsShooter.Enemies.Core
{
    public interface IEnemyFactory
    {
        // Добавили Transform[] patrolPoints в аргументы
        EnemyBrain Create(GameObject prefab, EnemyConfig config, Vector3 position, Quaternion rotation, Transform[] patrolPoints);
    }
}