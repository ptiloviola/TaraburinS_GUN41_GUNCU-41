using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        public BaseCore GetBaseById(string id)
        {
            if (string.IsNullOrEmpty(id) && _activeBases.Count > 0)
                return _activeBases.Values.First(); // Фолбэк

            if (_activeBases.TryGetValue(id, out var baseCore))
                return baseCore;

            return null;
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
    }
}

