using UnityEngine;

namespace Gameplay.Spawning
{
    public class EnemySpawnPoint : MonoBehaviour
    {
        [Tooltip("ID точки, совпадающий с тем, что указан в SquadData (например: 1, Main, North)")]
        public string PointId = "1";

        private void OnDrawGizmos()
        {
            // Рисуем красный кубик в редакторе, чтобы легко находить спавнер глазами
            Gizmos.color = Color.red;
            Gizmos.DrawCube(transform.position + Vector3.up * 0.5f, Vector3.one);
        }
    }
}