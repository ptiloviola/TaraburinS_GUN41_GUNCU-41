using UnityEngine;
using TpsShooter.Enemies.Configs;

namespace TpsShooter.Enemies.Core
{
    public interface IEnemyFactory
    {
        EnemyBrain Create(GameObject prefab, EnemyConfig config, Vector3 position, Quaternion rotation, Transform[] patrolPoints);
    }
}