using Gameplay.Towers.Data;
using UnityEngine;
using System;
using System.Collections.Generic;
using Gameplay.Towers.Behaviors;
using Gameplay.Towers.Visuals;
using Gameplay.Towers.Services;

namespace Gameplay.Towers
{
    public class TowerFacade : MonoBehaviour
    {
        [SerializeField] private TowerConfig _config;
        
        public int CurrentLevel { get; private set; }
        public TowerConfig Config => _config;
        public Vector2Int GridPosition { get; private set; }

        private readonly List<ITowerBehavior> _behaviors = new List<ITowerBehavior>();
        private ITowerVisuals _visuals; // НОВОЕ: Сохраняем ссылку на визуал
        
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

            var adapters = GetComponentsInChildren<IBehaviorAdapter>(true);
            WeaponAdapter foundWeaponAdapter = null;

            foreach (var adapter in adapters)
            {
                if (adapter is WeaponAdapter wa) 
                {
                    foundWeaponAdapter = wa;
                }
            }

            // НОВОЕ: Ищем визуал и сохраняем ссылку на него в поле
            _visuals = GetComponentInChildren<ITowerVisuals>(true);
            if (_visuals != null)
            {
                _visuals.Initialize(foundWeaponAdapter);
            }

            foreach (var adapter in adapters)
            {
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
            
            for (int i = 0; i < _behaviors.Count; i++)
            {
                _behaviors[i].Initialize(this); 
            }

            OnLevelChanged?.Invoke();
        }

        // --- НОВОЕ ПУБЛИЧНОЕ API ДЛЯ УПРАВЛЕНИЯ ВИЗУАЛОМ ---

        public void ShowRadiusPreview()
        {
            if (_visuals == null) return;
            
            TowerLevelData currentStats = GetCurrentStats();
            float currentRadius = TowerRadiusCalculator.GetMaxRadius(currentStats);
            float minRadius = TowerRadiusCalculator.GetMinRadius(currentStats);
            
            _visuals.ShowRadius(currentRadius, 0f, minRadius);
        }

        public void ShowUpgradePreview()
        {
            if (_visuals == null) return;
            
            TowerLevelData currentStats = GetCurrentStats();
            float currentRadius = TowerRadiusCalculator.GetMaxRadius(currentStats);
            float minRadius = TowerRadiusCalculator.GetMinRadius(currentStats);
            
            float nextRadius = 0f;
            if (CanUpgrade())
            {
                TowerLevelData nextStats = _config.Levels[CurrentLevel + 1];
                nextRadius = TowerRadiusCalculator.GetMaxRadius(nextStats);
            }
            
            _visuals.ShowRadius(currentRadius, nextRadius, minRadius);
        }

        public void HideRadiusPreview()
        {
            _visuals?.HideRadius();
        }

        private void OnDestroy()
        {
            foreach (var behavior in _behaviors)
            {
                if (behavior is BarracksController barracks)
                {
                    barracks.Cleanup();
                }
            }
        }
    }
}