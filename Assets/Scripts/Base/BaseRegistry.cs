using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Gameplay.Base
{
    public class BaseRegistry
    {
        private readonly Dictionary<string, BaseCore> _activeBases = new Dictionary<string, BaseCore>();

        public IEnumerable<BaseCore> ActiveBases => _activeBases.Values;

        public void Register(BaseCore baseCore)
        {
            if (!_activeBases.ContainsKey(baseCore.BaseId))
            {
                _activeBases.Add(baseCore.BaseId, baseCore);
#if UNITY_EDITOR
                Debug.Log($"<color=blue>[BaseRegistry] База зарегистрирована: {baseCore.BaseId}</color>");
#endif
            }
        }

        public void Unregister(BaseCore baseCore)
        {
            if (_activeBases.ContainsKey(baseCore.BaseId))
            {
                _activeBases.Remove(baseCore.BaseId);
            }
        }

        public BaseCore GetBaseById(string id)
        {
            if (_activeBases.TryGetValue(id, out var baseCore))
            {
                return baseCore;
            }
            
            return _activeBases.Count > 0 ? _activeBases.Values.First() : null;
        }
    }
}