using UnityEngine;

namespace MeatMushrooms.Environment.Strategies
{
    public class CenterCircleSpawnStrategy : IWolfSpawnStrategy
    {
        public Vector3 GetSpawnRaycastPosition(Vector3 centerPos, float mapRadius)
        {
            Vector2 randomCircle = Random.insideUnitCircle * (mapRadius * 0.4f);
            return centerPos + new Vector3(randomCircle.x, 50f, randomCircle.y);
        }
    }
}