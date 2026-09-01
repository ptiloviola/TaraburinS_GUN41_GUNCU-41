using UnityEngine;

namespace Infrastructure.Signals
{
    public struct SignalSpawnEnemyRequest
    {
        public string EnemyId;
        public Vector3 Position;
        public string TargetBaseId; // Опционально. Если пусто, режиссер направит сегмент на ближайшую базу.
    }
}