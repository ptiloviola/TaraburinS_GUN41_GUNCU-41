using UnityEngine;

namespace Gameplay.Infrastructure.Signals
{
    public struct SignalSpawnEnemyRequest
    {
        public string EnemyId;
        public Vector3 Position;
        public string TargetBaseId;
    }
}