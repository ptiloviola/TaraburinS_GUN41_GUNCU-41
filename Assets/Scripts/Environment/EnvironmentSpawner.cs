using UnityEngine;
using Unity.AI.Navigation;
using Zenject;
using System.Collections.Generic;

namespace MeatMushrooms.Environment
{
    public class EnvironmentSpawner : MonoBehaviour
    {
        public StageConfig Config;
        
        [Header("Настройки земли")]
        public MeshFilter GroundMeshFilter;
        public float NoiseScale = 5f; // Насколько "широкие" холмы
        public float HeightMultiplier = 1.5f; // Насколько высокие холмы

        [Header("Навигация")]
        public NavMeshSurface NavSurface;

        [Header("Проверка коллизий")]
        public float ObstacleCheckRadius = 1.5f; // Чтобы деревья не слипались
        public LayerMask ObstacleMask; // Наш слой Obstacle из прошлых уроков

        [Inject] private DiContainer _container;


        private void Start()
        {
            ResizeGround();     // 1. Растягиваем землю под размер уровня
            GenerateTerrain();  // 2. Искривляем рельеф
            SpawnObstacles();   // 3. Рассаживаем лес
            BakeNavMesh();
            SpawnCharacters();
        }

        private void GenerateTerrain()
        {
            if (GroundMeshFilter == null) return;

            // Берем меш земли (копию, чтобы не сломать исходный ассет)
            Mesh mesh = GroundMeshFilter.mesh; 
            Vector3[] vertices = mesh.vertices;

            // Случайное смещение "карты высот", чтобы каждая поляна была уникальной
            float offsetX = Random.Range(0f, 9999f);
            float offsetZ = Random.Range(0f, 9999f);

            for (int i = 0; i < vertices.Length; i++)
            {
                // Переводим локальные координаты в глобальные
                Vector3 worldPoint = GroundMeshFilter.transform.TransformPoint(vertices[i]);
                
                float xCoord = worldPoint.x / NoiseScale + offsetX;
                float zCoord = worldPoint.z / NoiseScale + offsetZ;
                
                // Шум Перлина возвращает значение от 0 до 1
                float y = Mathf.PerlinNoise(xCoord, zCoord) * HeightMultiplier;
                
                vertices[i].y = y;
            }

            // Применяем новые вершины
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            // ОБЯЗАТЕЛЬНО обновляем физический коллайдер, чтобы волки ходили по холмам, а не под ними
            MeshCollider col = GroundMeshFilter.GetComponent<MeshCollider>();
            if (col != null) col.sharedMesh = mesh;
        }

        private void SpawnObstacles()
        {
            // 1. ДЕРЕВЬЯ: 80% по периметру (густо), 20% в центре (редко)
            int perimeterTrees = Mathf.RoundToInt(Config.TreeCount * 0.8f);
            int centerTrees = Config.TreeCount - perimeterTrees;
            
            // Периметр: от 70% до 100% радиуса карты
            SpawnObjects(Config.TreePrefabs, perimeterTrees, Config.MapRadius * 0.7f, Config.MapRadius);
            // Центр: от 2 метров до 70% радиуса
            SpawnObjects(Config.TreePrefabs, centerTrees, 2f, Config.MapRadius * 0.7f);

            // 2. КАМНИ: 80% в центре (густо), 20% по краям (редко)
            int centerRocks = Mathf.RoundToInt(Config.RockCount * 0.8f);
            int perimeterRocks = Config.RockCount - centerRocks;
            
            // Центр для камней: от 2 метров до 50% радиуса
            SpawnObjects(Config.RockPrefabs, centerRocks, 2f, Config.MapRadius * 0.5f);
            // Периметр для камней
            SpawnObjects(Config.RockPrefabs, perimeterRocks, Config.MapRadius * 0.5f, Config.MapRadius);
        }

        private void SpawnObjects(GameObject[] prefabs, int count, float minRadius, float maxRadius)
        {
            if (prefabs == null || prefabs.Length == 0 || GroundMeshFilter == null) return;

            // ИСПРАВЛЕНИЕ БАГА: Теперь мы точно берем центр нашей земли, где бы она ни находилась
            Vector3 centerPos = GroundMeshFilter.transform.position; 

            int spawned = 0;
            int attempts = 0; 

            while (spawned < count && attempts < count * 10)
            {
                attempts++;
                
                Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minRadius, maxRadius);
                
                // Прибавляем координаты центра земли и поднимаем луч повыше (на 50 метров)
                Vector3 raycastStart = centerPos + new Vector3(randomCircle.x, 50f, randomCircle.y);

                // Пускаем луч на 100 метров вниз
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

            // Считаем нужный масштаб на основе радиуса из конфига
            // Формула: (Радиус * 2) / 10
            float scaleFactor = (Config.MapRadius * 2f) / 10f;

            // Применяем масштаб к Plane. 
            // ВАЖНО: Ось Y оставляем равной 1, иначе наши сгенерированные холмы растянутся в гигантские скалы!
            GroundMeshFilter.transform.localScale = new Vector3(scaleFactor, 1f, scaleFactor);
            
            Debug.Log($"<color=green>[EnvironmentSpawner]</color> Земля масштабирована. Новый Scale: {scaleFactor}");
        }
        private void BakeNavMesh()
        {
            if (NavSurface != null)
            {
                // Эта команда сканирует все объекты слоев Ground и Obstacle и строит синюю сетку
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

            // --- 1. СПАВН ШАПОЧКИ (В самом низу карты: Z = -Radius) ---
            // Отступаем 20% от края, чтобы она не появилась прямо в текстуре границы
            Vector3 playerRayStart = centerPos + new Vector3(0, 50f, -Config.MapRadius * 0.8f);
            if (Physics.Raycast(playerRayStart, Vector3.down, out RaycastHit playerHit, 100f))
            {
                // Используем Zenject для спавна!
                _container.InstantiatePrefab(Config.PlayerPrefab, playerHit.point, Quaternion.Euler(0, 0, 0), null);
            }

            // --- 2. СПАВН ВЫХОДА (В самом верху карты: Z = +Radius) ---
            Vector3 exitRayStart = centerPos + new Vector3(0, 50f, Config.MapRadius * 0.8f);
            if (Physics.Raycast(exitRayStart, Vector3.down, out RaycastHit exitHit, 100f))
            {
                // Выход не использует Zenject, спавним обычным Instantiate
                Instantiate(Config.ExitPrefab, exitHit.point, Quaternion.identity, transform);
            }

            // --- 3. СПАВН ВОЛКОВ (В центре карты, в радиусе 40% от размера поляны) ---
            int spawnedWolves = 0;
            int attempts = 0;
            List<Vector3> wolfPositions = new List<Vector3>(); // Запоминаем, где стоят волки

            while (spawnedWolves < Config.WolfCount && attempts < 100)
            {
                attempts++;
                Vector2 randomCircle = Random.insideUnitCircle * (Config.MapRadius * 0.4f);
                Vector3 rayStart = centerPos + new Vector3(randomCircle.x, 50f, randomCircle.y);

                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 100f))
                {
                    // Проверяем, нет ли здесь камня или дерева
                    if (!Physics.CheckSphere(hit.point, ObstacleCheckRadius, ObstacleMask))
                    {
                        // Проверяем, не слишком ли близко к другому волку (минимум 3 метра)
                        bool isTooClose = false;
                        foreach (var wPos in wolfPositions)
                        {
                            if (Vector3.Distance(hit.point, wPos) < 3f)
                            {
                                isTooClose = true;
                                break;
                            }
                        }

                        if (!isTooClose)
                        {
                            // Спавним волка через Zenject
                            _container.InstantiatePrefab(Config.WolfPrefab, hit.point, Quaternion.identity, null);
                            
                            wolfPositions.Add(hit.point);
                            spawnedWolves++;
                        }
                    }
                }
            }
            
            Debug.Log($"<color=magenta>[Spawner]</color> Заспавнено {spawnedWolves} волков, Шапочка и Выход.");
        }



    }
}