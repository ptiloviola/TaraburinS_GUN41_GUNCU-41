using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI; // НОВОЕ: Для работы с путями

namespace Gameplay.Base
{
    public class BaseRegistry
    {
        // Теперь это словарь!
        private readonly Dictionary<string, BaseCore> _activeBases = new Dictionary<string, BaseCore>();

        // public IReadOnlyList<BaseCore> ActiveBases => _activeBases;

        public void Register(BaseCore baseCore)
        {
            if (!_activeBases.ContainsKey(baseCore.BaseId))
            {
                _activeBases.Add(baseCore.BaseId, baseCore);
                Debug.Log($"<color=blue>[BaseRegistry] База зарегистрирована: {baseCore.BaseId}</color>");
            }
        }

        public void Unregister(BaseCore baseCore)
        {
            if (_activeBases.ContainsKey(baseCore.BaseId))
                _activeBases.Remove(baseCore.BaseId);
        }
        // Фолбэк-метод для текущей логики: берем просто первую доступную базу
        // УМНЫЙ ПОИСК: Если ID пустой - даем первую попавшуюся. Иначе ищем по ID.
        // Обновленный метод поиска. Теперь он просит координаты спавна для расчета пути!
        public BaseCore GetBaseById(string id, Vector3 spawnPosition)
        {
            // Если стоит наш спец-тег из конфига -> запускаем алгоритм поиска ближайшей
            if (id == "[Ближайшая по пути]")
            {
                return GetNearestBaseByPath(spawnPosition);
            }

            // это если не выставляем id базы
            // if (string.IsNullOrEmpty(id) && _activeBases.Count > 0)
            //     return _activeBases.Values.First(); // Фолбэк

            if (_activeBases.TryGetValue(id, out var baseCore))
                return baseCore;

            // Если вдруг что-то пошло не так, возвращаем первую попавшуюся
            return _activeBases.Count > 0 ? _activeBases.Values.First() : null;
        }
        // ЗАДЕЛ НА БУДУЩЕЕ: враг сможет динамически выбирать ближайшую к нему базу!
        // public BaseCore GetNearestBase(Vector3 enemyPosition)
        // {
        //     BaseCore nearest = null;
        //     float minDistance = float.MaxValue;
        //     foreach (var baseCore in _activeBases)
        //     {
        //         float sqrDist = (baseCore.transform.position - enemyPosition).sqrMagnitude;
        //         if (sqrDist < minDistance)
        //         {
        //             minDistance = sqrDist;
        //             nearest = baseCore;
        //         }
        //     }
        //     return nearest;
        // }


        // --- ТОТ САМЫЙ АЛГОРИТМ УМНОГО ПОИСКА ---
        private BaseCore GetNearestBaseByPath(Vector3 spawnPosition)
        {
            BaseCore nearestBase = null;
            float minPathLength = float.MaxValue;
            
            // Вспомогательный объект Unity для хранения точек маршрута
            NavMeshPath path = new NavMeshPath(); 

            foreach (var baseCore in _activeBases.Values)
            {
                // Просим Unity рассчитать маршрут от спавна до конкретной базы
                if (NavMesh.CalculatePath(spawnPosition, baseCore.transform.position, NavMesh.AllAreas, path))
                {
                    // Считаем физическую длину этого маршрута
                    float pathLength = CalculatePathLength(path);
                    
                    if (pathLength < minPathLength)
                    {
                        minPathLength = pathLength;
                        nearestBase = baseCore;
                    }
                }
            }

            // Возвращаем найденную базу (или дефолтную, если пути вообще нет)
            return nearestBase != null ? nearestBase : _activeBases.Values.FirstOrDefault();
        }

        // Вспомогательная математика: складываем длину всех отрезков пути
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

