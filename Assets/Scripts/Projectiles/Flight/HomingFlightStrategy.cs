using UnityEngine;
using Gameplay.Projectiles.Contracts;

namespace Gameplay.Projectiles.Flight
{
    public class HomingFlightStrategy : MonoBehaviour, IFlightStrategy
    {
        [SerializeField] private Vector3 _targetOffset = new Vector3(0f, 0.5f, 0f);

        public void Initialize(Transform p, Transform t) {}

        public bool ExecuteFlight(Transform projectile, Transform target, float speed, float hitDistance)
        {
            if (target == null) return false;

            Vector3 aimPosition = target.position + _targetOffset;
            
            projectile.position = Vector3.MoveTowards(projectile.position, aimPosition, speed * Time.deltaTime);
            projectile.LookAt(aimPosition);

            float sqrDistance = (projectile.position - aimPosition).sqrMagnitude;
            return sqrDistance <= (hitDistance * hitDistance); // Если долетели — возвращаем true
        }
    }
}