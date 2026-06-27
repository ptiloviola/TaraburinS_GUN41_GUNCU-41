using UnityEngine;

namespace VacuumSim.Spawning
{
    [RequireComponent(typeof(BoxCollider))]
    public class SpawnArea : MonoBehaviour
    {
        [Header("Настройки зоны")]
        [Tooltip("Коллайдер, определяющий зону под потолком")]
        [SerializeField] private BoxCollider _spawnVolume;
        
        [Tooltip("Слой пола, куда мусору падать МОЖНО")]
        [SerializeField] private LayerMask _validFloorMask;
        
        [Tooltip("Слои мебели/препятствий, которых нужно ИЗБЕГАТЬ")]
        [SerializeField] private LayerMask _obstacleMask;

        private void Reset()
        {
            _spawnVolume = GetComponent<BoxCollider>();
            _spawnVolume.isTrigger = true; 
        }

        public bool TryGetValidSpawnPoint(out Vector3 point)
        {
            point = Vector3.zero;
            
            Bounds bounds = _spawnVolume.bounds;
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomZ = Random.Range(bounds.min.z, bounds.max.z);
            
            Vector3 origin = new Vector3(randomX, bounds.max.y, randomZ);
            
            LayerMask combinedMask = _validFloorMask | _obstacleMask;

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, Mathf.Infinity, combinedMask))
            {
                if ((_validFloorMask.value & (1 << hit.collider.gameObject.layer)) != 0)
                {
                    point = hit.point;
                    return true;
                }
                
            }
            
            return false;
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_spawnVolume == null) return;
            
            Gizmos.color = new Color(0, 0.5f, 1f, 0.3f);
            Gizmos.DrawCube(_spawnVolume.bounds.center, _spawnVolume.bounds.size);
            Gizmos.DrawWireCube(_spawnVolume.bounds.center, _spawnVolume.bounds.size);
        }
        #endif
    }
}