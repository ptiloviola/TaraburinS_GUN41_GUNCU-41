using UnityEngine;
using Unity.AI.Navigation;
using Zenject;
using System.Collections.Generic;
using MeatMushrooms.Player;
using MeatMushrooms.Core;
using MeatMushrooms.Environment.Strategies;

namespace MeatMushrooms.Environment
{
    public class EnvironmentSpawner : MonoBehaviour
    {
        public StageConfig Config;
        
        [Header("Настройки земли")]
        public MeshFilter GroundMeshFilter;
        public float NoiseScale = 5f; 
        public float HeightMultiplier = 1.5f; 

        [Header("Навигация")]
        public NavMeshSurface NavSurface;

        [Header("Проверка коллизий")]
        public float ObstacleCheckRadius = 1.5f; 
        public LayerMask ObstacleMask; 

        [Header("Камера")]
        public CameraSystem.CameraController MainCamera; 

        [Inject] private DiContainer _container;
        [Inject] private PlayerRegistry _playerRegistry;

        private int _levelBonus => GameSession.CurrentLevel - 1;
        
        private float _currentRadius => Config.MapRadius + (_levelBonus * Config.RadiusIncrement);
        private int _currentTrees => Config.TreeCount + (_levelBonus * Config.TreeIncrement);
        private int _currentRocks => Config.RockCount + (_levelBonus * Config.RockIncrement);
        private int _currentWolves => Config.WolfCount + (_levelBonus * Config.WolfIncrement);

        private void Start()
        {
            ResizeGround();     
            GenerateTerrain();  
            SpawnObstacles();   
            BakeNavMesh();      
            SpawnCharacters();  
        }

        private void GenerateTerrain()
        {
            if (GroundMeshFilter == null) return;

            Mesh mesh = GroundMeshFilter.mesh; 
            Vector3[] vertices = mesh.vertices;

            float offsetX = Random.Range(0f, 9999f);
            float offsetZ = Random.Range(0f, 9999f);

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 worldPoint = GroundMeshFilter.transform.TransformPoint(vertices[i]);
                
                float xCoord = worldPoint.x / NoiseScale + offsetX;
                float zCoord = worldPoint.z / NoiseScale + offsetZ;
                
                float y = Mathf.PerlinNoise(xCoord, zCoord) * HeightMultiplier;
                
                vertices[i].y = y;
            }

            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            MeshCollider col = GroundMeshFilter.GetComponent<MeshCollider>();
            if (col != null) col.sharedMesh = mesh;
        }

        private void SpawnObstacles()
        {
            int perimeterTrees = Mathf.RoundToInt(_currentTrees * 0.8f);
            int centerTrees = _currentTrees - perimeterTrees;
            
            SpawnObjects(Config.TreePrefabs, perimeterTrees, _currentRadius * 0.7f, _currentRadius);
            SpawnObjects(Config.TreePrefabs, centerTrees, 2f, _currentRadius * 0.7f);

            int centerRocks = Mathf.RoundToInt(_currentRocks * 0.8f);
            int perimeterRocks = _currentRocks - centerRocks;
            
            SpawnObjects(Config.RockPrefabs, centerRocks, 2f, _currentRadius * 0.5f);
            SpawnObjects(Config.RockPrefabs, perimeterRocks, _currentRadius * 0.5f, _currentRadius);
        }

        private void SpawnObjects(GameObject[] prefabs, int count, float minRadius, float maxRadius)
        {
            if (prefabs == null || prefabs.Length == 0 || GroundMeshFilter == null) return;

            Vector3 centerPos = GroundMeshFilter.transform.position; 

            int spawned = 0;
            int attempts = 0; 

            while (spawned < count && attempts < count * 10)
            {
                attempts++;
                
                Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minRadius, maxRadius);
                Vector3 raycastStart = centerPos + new Vector3(randomCircle.x, 50f, randomCircle.y);

                if (Physics.Raycast(raycastStart, Vector3.down, out RaycastHit hit, 100f))
                {
                    Vector3 spawnPos = hit.point;

                    if (!Physics.CheckSphere(spawnPos, ObstacleCheckRadius, ObstacleMask))
                    {
                        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                        Quaternion randomRot = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                        
                        Instantiate(prefab, spawnPos, randomRot, transform);
                        spawned++;
                    }
                }
            }
            
            Debug.Log($"<color=green>[EnvironmentSpawner]</color> Сгенерировано: {spawned}/{count}. Попыток: {attempts}");
        }

        private void ResizeGround()
        {
            if (GroundMeshFilter == null) return;

            float scaleFactor = (_currentRadius * 2f) / 10f;

            GroundMeshFilter.transform.localScale = new Vector3(scaleFactor, 1f, scaleFactor);
            
            Debug.Log($"<color=green>[EnvironmentSpawner]</color> Земля масштабирована. Новый Scale: {scaleFactor}");
        }

        private void BakeNavMesh()
        {
            if (NavSurface != null)
            {
                NavSurface.BuildNavMesh();
                Debug.Log("<color=cyan>[EnvironmentSpawner]</color> NavMesh успешно сгенерирован для новой поляны!");
            }
            else
            {
                Debug.LogWarning("NavMeshSurface не назначен в генераторе!");
            }
        }

        private void SpawnCharacters()
        {
            Vector3 centerPos = GroundMeshFilter.transform.position;

            Vector3 playerRayStart = centerPos + new Vector3(0, 50f, -_currentRadius * 0.8f);
            if (Physics.Raycast(playerRayStart, Vector3.down, out RaycastHit playerHit, 100f))
            {
                GameObject playerInstance = _container.InstantiatePrefab(Config.PlayerPrefab, playerHit.point, Quaternion.Euler(0, 0, 0), null);
                
                _playerRegistry.Register(playerInstance);

                if (MainCamera != null)
                {
                    MainCamera.SetTarget(playerInstance.transform);
                }
            }

            Vector3 exitRayStart = centerPos + new Vector3(0, 50f, _currentRadius * 0.8f);
            if (Physics.Raycast(exitRayStart, Vector3.down, out RaycastHit exitHit, 100f))
            {
                Instantiate(Config.ExitPrefab, exitHit.point, Quaternion.identity, transform);
            }

            int spawnedWolves = 0;
            int attempts = 0;
            List<Vector3> wolfPositions = new List<Vector3>();

            IWolfSpawnStrategy spawnStrategy = WolfSpawnStrategyFactory.Create(Config.WolfLayout);

            while (spawnedWolves < _currentWolves && attempts < _currentWolves * 25)
            {
                attempts++;
                
                Vector3 rayStart = spawnStrategy.GetSpawnRaycastPosition(centerPos, _currentRadius);

                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 100f))
                {
                    if (!Physics.CheckSphere(hit.point, ObstacleCheckRadius, ObstacleMask))
                    {
                        bool isTooClose = false;
                        foreach (var wPos in wolfPositions)
                        {
                            if (Vector3.Distance(hit.point, wPos) < 4f)
                            {
                                isTooClose = true;
                                break;
                            }
                        }

                        if (!isTooClose)
                        {
                            Quaternion randomWolfRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                            
                            _container.InstantiatePrefab(Config.WolfPrefab, hit.point, randomWolfRotation, null);
                            
                            wolfPositions.Add(hit.point);
                            spawnedWolves++;
                        }
                    }
                }
            }
            
            Debug.Log($"<color=magenta>[Spawner]</color> Заспавнено {spawnedWolves} волков (Стратегия: {Config.WolfLayout}).");
        }
    }
}