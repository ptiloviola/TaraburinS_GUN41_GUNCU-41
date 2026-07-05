using UnityEngine;

namespace MeatMushrooms.Environment.Strategies
{
    public interface IWolfSpawnStrategy
    {
        Vector3 GetSpawnRaycastPosition(Vector3 centerPos, float mapRadius);
    }
}