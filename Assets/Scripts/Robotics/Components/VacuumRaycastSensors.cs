using UnityEngine;
using VacuumSim.Robotics.Contracts;
using Zenject;
using VacuumSim.Robotics.Configs;

namespace VacuumSim.Robotics.Components
{
    public class VacuumRaycastSensors : MonoBehaviour, IVacuumSensors
    {
        [Header("Настройки физики")]
        [Tooltip("Слои, которые сенсор считает стенами")]
        [SerializeField] private LayerMask _obstacleMask;

        private Vector3 _originOffset = new Vector3(0, 0.1f, 0);

        [Header("Данные для редактора (Gizmos)")]
        // Оставляем SerializeField ТОЛЬКО для того, чтобы Unity видела конфиг до старта игры
        [SerializeField] private VacuumConfig _config;

        [Inject]
        public void Construct(VacuumConfig config)
        {
            _config = config;
        }



        public bool IsObstacleAhead() 
        {
            return CheckDirection(transform.forward, Color.red);
        }

        public bool IsObstacleRight() 
        {
            Vector3 direction = Quaternion.Euler(0, _config.SideAngle, 0) * transform.forward;
            return CheckDirection(direction, Color.yellow);
        }

        public bool IsObstacleLeft() 
        {
            Vector3 direction = Quaternion.Euler(0, -_config.SideAngle, 0) * transform.forward;
            return CheckDirection(direction, Color.yellow);
        }

        private bool CheckDirection(Vector3 direction, Color debugColor)
        {
            Vector3 origin = transform.position + _originOffset;
            Ray ray = new Ray(origin, direction);
            
            Debug.DrawRay(ray.origin, ray.direction * _config.RayDistance, debugColor, 0.5f);
            
            if (Physics.SphereCast(ray, _config.SphereRadius, out RaycastHit hit, _config.RayDistance, _obstacleMask))
            {
                Debug.Log($"[Sensors] SphereCast задел: '{hit.collider.name}' на дист {hit.distance}м", hit.collider.gameObject);
                return true;
            }
            return false;
        }

        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {

            Vector3 origin = transform.position + _originOffset;
            Vector3 forward = transform.forward;
            Vector3 right = Quaternion.Euler(0, _config.SideAngle, 0) * forward;
            Vector3 left = Quaternion.Euler(0, -_config.SideAngle, 0) * forward;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(origin, forward * _config.RayDistance);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(origin, right * _config.RayDistance);
            Gizmos.DrawRay(origin, left * _config.RayDistance);

            Gizmos.DrawWireSphere(origin + forward * _config.RayDistance, _config.SphereRadius);
        }
        #endif
    }
}

