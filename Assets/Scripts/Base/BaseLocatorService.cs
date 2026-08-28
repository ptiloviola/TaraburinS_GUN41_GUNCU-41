using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.Base
{
    public class BaseLocatorService
    {
        public const string NearestByPathTag = "[Ближайшая по пути]";
        
        private readonly BaseRegistry _baseRegistry;

        public BaseLocatorService(BaseRegistry baseRegistry)
        {
            _baseRegistry = baseRegistry;
        }

        public BaseCore LocateTargetBase(string targetId, Vector3 spawnPosition)
        {
            if (targetId == NearestByPathTag)
            {
                return GetNearestBaseByPath(spawnPosition);
            }

            return _baseRegistry.GetBaseById(targetId);
        }

        private BaseCore GetNearestBaseByPath(Vector3 spawnPosition)
        {
            BaseCore nearestBase = null;
            float minPathLength = float.MaxValue;
            
            NavMeshPath path = new NavMeshPath(); 

            foreach (var baseCore in _baseRegistry.ActiveBases)
            {
                if (NavMesh.CalculatePath(spawnPosition, baseCore.transform.position, NavMesh.AllAreas, path))
                {
                    float pathLength = CalculatePathLength(path);
                    
                    if (pathLength < minPathLength)
                    {
                        minPathLength = pathLength;
                        nearestBase = baseCore;
                    }
                }
            }

            return nearestBase != null ? nearestBase : _baseRegistry.ActiveBases.FirstOrDefault();
        }

        private float CalculatePathLength(NavMeshPath path)
        {
            if (path.corners.Length < 2) return 0f;
            
            float length = 0f;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                length += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }
            return length;
        }
    }
}