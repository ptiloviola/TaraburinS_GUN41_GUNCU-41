using UnityEngine;

namespace TpsShooter.Effects
{
    public interface IVFXService
    {
        // Метод для вызова трассера пули
        void SpawnTracer(Vector3 startPoint, Vector3 endPoint);
        
        // Метод для вызова эффектов попадания (позже добавим сюда кровь и искры)
        // void SpawnImpact(Vector3 position, Vector3 normal, bool isEnemy); 
    }
}