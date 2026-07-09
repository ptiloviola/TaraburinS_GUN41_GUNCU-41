using UnityEngine;

namespace Infrastructure.Interfaces
{
    public interface IShooter
    {
        bool TryShot(float spread, LayerMask mask, out RaycastHit hitInfo, out Vector3 targetPoint);
    }
}