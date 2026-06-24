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
            // Автоматически подхватит коллайдер при добавлении скрипта
            _spawnVolume = GetComponent<BoxCollider>();
            _spawnVolume.isTrigger = true; 
        }

        // Метод возвращает true, если нашел точку, и саму точку через out-параметр
        public bool TryGetValidSpawnPoint(out Vector3 point)
        {
            point = Vector3.zero;
            
            // 1. Выбираем случайные координаты X и Z внутри коллайдера
            Bounds bounds = _spawnVolume.bounds;
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomZ = Random.Range(bounds.min.z, bounds.max.z);
            
            // Точка старта луча (самая верхняя плоскость коллайдера)
            Vector3 origin = new Vector3(randomX, bounds.max.y, randomZ);
            
            // 2. Объединяем маски с помощью побитового ИЛИ (|)
            LayerMask combinedMask = _validFloorMask | _obstacleMask;

            // 3. Стреляем лучом строго вниз
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, Mathf.Infinity, combinedMask))
            {
                // Проверяем, принадлежит ли объект, в который мы попали, слою пола
                // (Побитовый сдвиг проверяет совпадение слоев)
                if ((_validFloorMask.value & (1 << hit.collider.gameObject.layer)) != 0)
                {
                    // Бинго! Луч попал в голый пол.
                    point = hit.point;
                    return true;
                }
                
                // Если мы попали сюда, значит луч уперся в стол, диван или стену.
                // Точка невалидна, возвращаем false.
            }
            
            return false;
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_spawnVolume == null) return;
            
            // Рисуем красивую полупрозрачную синюю зону под потолком в редакторе
            Gizmos.color = new Color(0, 0.5f, 1f, 0.3f);
            Gizmos.DrawCube(_spawnVolume.bounds.center, _spawnVolume.bounds.size);
            Gizmos.DrawWireCube(_spawnVolume.bounds.center, _spawnVolume.bounds.size);
        }
        #endif
    }
}