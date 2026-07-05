using UnityEngine;

namespace MeatMushrooms.Environment.Strategies
{
    public class LineAcrossMapSpawnStrategy : IWolfSpawnStrategy
    {
        public Vector3 GetSpawnRaycastPosition(Vector3 centerPos, float mapRadius)
        {
            float randomX = Random.Range(-mapRadius * 0.8f, mapRadius * 0.8f);
            float randomZ = Random.Range(-3f, 3f); 
            return centerPos + new Vector3(randomX, 50f, randomZ);
        }
    }
}