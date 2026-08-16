using UnityEngine;

namespace TpsShooter.Effects
{
    public interface IVFXService
    {
        // Метод для вызова трассера пули
        void SpawnTracer(Vector3 startPoint, Vector3 endPoint);
        
        void SpawnImpact(Vector3 position, Vector3 normal, bool isEnemy);
    }
}