using UnityEngine;

namespace MeatMushrooms.Environment.Strategies
{
    public interface IWolfSpawnStrategy
    {
        // Метод возвращает начальную точку для луча спавна
        Vector3 GetSpawnRaycastPosition(Vector3 centerPos, float mapRadius);
    }
}