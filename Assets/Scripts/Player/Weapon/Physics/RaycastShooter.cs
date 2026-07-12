using UnityEngine;
using Infrastructure.Interfaces;
namespace Player.Weapon.Physics
{
    public class RaycastShooter : IShooter
    {
        private readonly Transform _cameraTransform;

        public RaycastShooter(Transform cameraTransform)
        {
            _cameraTransform = cameraTransform;
        }

        public bool TryShot(float spread, LayerMask mask, out RaycastHit hitInfo, out Vector3 targetPoint)
        {
            Vector2 spreadOffset = Random.insideUnitCircle * spread;
            
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f + spreadOffset.x, 0.5f + spreadOffset.y, 0f));

            if (UnityEngine.Physics.Raycast(ray, out hitInfo, 100f, mask))
            {
                targetPoint = hitInfo.point;
                return true;
            }

            targetPoint = ray.GetPoint(50f);
            return false;
        }
    }
}
