using Gameplay.Towers.Data;
using UnityEngine;
using System;
using System.Collections.Generic;
using Gameplay.Towers.Behaviors;

namespace Gameplay.Towers
{
    public class TowerFacade : MonoBehaviour
    {
        [SerializeField] private TowerConfig _config;
        
        public int CurrentLevel { get; private set; }
        public TowerConfig Config => _config;
        public Vector2Int GridPosition { get; private set; }

        private readonly List<ITowerBehavior> _behaviors = new List<ITowerBehavior>();
        
        public event Action OnLevelChanged;

        private void Start()
        {
            if (_behaviors.Count == 0)
            {
                if (_config != null) Initialize(_config, Vector2Int.zero);
#if UNITY_EDITOR
                else Debug.LogError($"[TowerFacade] На объекте {name} нет TowerConfig!");
#endif
            }
        }

        public void Initialize(TowerConfig config, Vector2Int gridPos)
        {
            _config = config;
            GridPosition = gridPos;
            
            _behaviors.Clear();

            // Ищем глупые адаптеры (WeaponAdapter, BarracksAdapter и т.д.)
            var adapters = GetComponentsInChildren<IBehaviorAdapter>(true);

            foreach (var adapter in adapters)
            {
                // Адаптер рождает чистый C# класс (например, AttackController)
                ITowerBehavior behavior = adapter.CreateBehavior();
                behavior.Initialize(this);
                _behaviors.Add(behavior);
            }
        }

        private void Update()
        {
            for (int i = 0; i < _behaviors.Count; i++)
            {
                _behaviors[i].Tick(Time.deltaTime);
            }
        }

        public TowerLevelData GetCurrentStats() => _config.Levels[CurrentLevel];
        public bool CanUpgrade() => CurrentLevel < _config.MaxLevel;

        public void Upgrade()
        {
            if (!CanUpgrade()) return;
            CurrentLevel++;
            
            // Перезапускаем чистые классы, чтобы они прочитали новые статы
            for (int i = 0; i < _behaviors.Count; i++)
            {
                _behaviors[i].Initialize(this); 
            }

            OnLevelChanged?.Invoke();
        }
        private void OnDestroy()
        {
            foreach (var behavior in _behaviors)
            {
                // Проверяем, реализует ли контроллер очистку (паттерн Type Checking)
                if (behavior is BarracksController barracks)
                {
                    barracks.Cleanup();
                }
            }
        }
    }
}