using UnityEngine;

namespace Infrastructure.Interfaces
{
    public interface IImpactHandler
    {
        void HandleImpact(Vector3 startPoint, Vector3 targetPoint, Collider targetCollider, Vector3 hitNormal);
    }
}
