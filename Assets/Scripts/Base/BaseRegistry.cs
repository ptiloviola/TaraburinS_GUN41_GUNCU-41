using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Base
{
    public class BaseRegistry
    {
        private readonly List<BaseCore> _activeBases = new List<BaseCore>();

        public IReadOnlyList<BaseCore> ActiveBases => _activeBases;

        public void Register(BaseCore baseCore)
        {
            if (!_activeBases.Contains(baseCore))
            {
                _activeBases.Add(baseCore);
                Debug.Log($"<color=blue>[BaseRegistry] База зарегистрирована! Всего баз: {_activeBases.Count}</color>");
            }
        }
        public void Unregister(BaseCore baseCore)
        {
            if (_activeBases.Contains(baseCore))
            {
                _activeBases.Remove(baseCore);
                Debug.Log($"<color=blue>[BaseRegistry] База удалена. Осталось баз: {_activeBases.Count}</color>");
            }
        }
        // Фолбэк-метод для текущей логики: берем просто первую доступную базу
        public BaseCore GetMainBase()
        {
            if (_activeBases.Count > 0) return _activeBases[0];
            return null;
        }
        // ЗАДЕЛ НА БУДУЩЕЕ: враг сможет динамически выбирать ближайшую к нему базу!
        public BaseCore GetNearestBase(Vector3 enemyPosition)
        {
            BaseCore nearest = null;
            float minDistance = float.MaxValue;
            foreach (var baseCore in _activeBases)
            {
                float sqrDist = (baseCore.transform.position - enemyPosition).sqrMagnitude;
                if (sqrDist < minDistance)
                {
                    minDistance = sqrDist;
                    nearest = baseCore;
                }
            }
            return nearest;
        }
    }
}

