using Gameplay.Towers.Data;
using UnityEngine;
using System.Collections.Generic;
using Gameplay.Towers.Behaviors;
using Gameplay.Towers.Visuals;
using Gameplay.Towers.Services;
using Zenject;
using Gameplay.Infrastructure.Signals;

namespace Gameplay.Towers
{
    public class TowerFacade : MonoBehaviour
    {
        [SerializeField] private TowerConfig _config;
        
        public int CurrentLevel { get; private set; }
        public TowerConfig Config => _config;
        public Vector2Int GridPosition { get; private set; }

        private readonly List<ITowerBehavior> _behaviors = new List<ITowerBehavior>();
        private ITowerVisuals _visuals;
        
        private SignalBus _signalBus;
        
        private bool _isActive = true; 

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void Start()
        {
            if (_behaviors.Count == 0)
            {
                if (_config != null) Initialize(_config, 0, Vector2Int.zero);
#if UNITY_EDITOR
                else Gameplay.Tools.GameLogger.LogError($"[TowerFacade] На объекте {name} нет TowerConfig!");
#endif
            }
            
            _signalBus?.Subscribe<SignalLevelWon>(HandleLevelWon);
            _signalBus?.Subscribe<SignalGameOver>(HandleGameOver);
        }

        public void Initialize(TowerConfig config, int level, Vector2Int gridPos)
        {
            _config = config;
            CurrentLevel = level;
            GridPosition = gridPos;
            _isActive = true;
            
            _behaviors.Clear();

            var adapters = GetComponentsInChildren<IBehaviorAdapter>(true);
            WeaponAdapter foundWeaponAdapter = null;

            foreach (var adapter in adapters)
            {
                if (adapter is WeaponAdapter wa) foundWeaponAdapter = wa;
            }

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
            if (!_isActive) return;

            for (int i = 0; i < _behaviors.Count; i++)
            {
                _behaviors[i].Tick(Time.deltaTime);
            }
        }

        private void HandleLevelWon()
        {
            _isActive = false;
            _visuals?.PlayVictoryAnimation();
        }

        private void HandleGameOver()
        {
            _isActive = false;
        }

        public TowerLevelData GetCurrentStats() => _config.Levels[CurrentLevel];
        public bool CanUpgrade() => CurrentLevel < _config.MaxLevel;

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
            _signalBus?.TryUnsubscribe<SignalLevelWon>(HandleLevelWon);
            _signalBus?.TryUnsubscribe<SignalGameOver>(HandleGameOver);

            foreach (var behavior in _behaviors)
            {
                behavior.Cleanup();
            }
        }
    }
}