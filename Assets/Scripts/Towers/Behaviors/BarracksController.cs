using System.Collections.Generic;
using UnityEngine;
using Gameplay.Towers.Data.Modules;
using Gameplay.Units;

namespace Gameplay.Towers.Behaviors
{
    public class BarracksController : ITowerBehavior
    {
        private readonly BarracksAdapter _adapter;
        private TowerFacade _facade;
        private BarracksModuleDescriptor _module;
        
        private readonly List<DefenderFacade> _activeDefenders = new List<DefenderFacade>();
        private float _respawnTimer;
        private Vector3 _rallyPoint;

        public BarracksController(BarracksAdapter adapter)
        {
            _adapter = adapter;
        }

        public void Initialize(TowerFacade facade)
        {
            _facade = facade;
            _module = _facade.GetCurrentStats().Barracks;
            
            if (_module == null || _module.DefenderData == null) return;

            _rallyPoint = _adapter.Center + _adapter.Forward * (_module.RallyPointRadius * 0.5f);
            _respawnTimer = _module.RespawnCooldown;

            foreach (var defender in _activeDefenders)
            {
                if (defender != null && defender.gameObject.activeInHierarchy)
                {
                    defender.InitConfig(_module.DefenderData);
                }
            }
        }

        public void Tick(float deltaTime)
        {
            if (_module == null || _adapter.DefenderFactory == null) return;
            
            if (_activeDefenders.Count < _module.MaxDefenders)
            {
                _respawnTimer -= deltaTime;
                if (_respawnTimer <= 0f)
                {
                    SpawnDefender();
                    _respawnTimer = _module.RespawnCooldown;
                }
            }
        }

        private void SpawnDefender()
        {
            var defender = _adapter.DefenderFactory.Create(_module.DefenderData.DefenderId);

            if (UnityEngine.AI.NavMesh.SamplePosition(_adapter.SpawnPoint, out UnityEngine.AI.NavMeshHit hit, 3f, UnityEngine.AI.NavMesh.AllAreas))
            {
                defender.WarpTo(hit.position);
            }
            else
            {
                defender.WarpTo(_adapter.Center); 
            }

            defender.InitConfig(_module.DefenderData);

            Vector2 rnd = Random.insideUnitCircle * 1.5f;
            Vector3 randomOffset = new Vector3(rnd.x, 0, rnd.y);
            
            defender.SendToRallyPoint(_rallyPoint + randomOffset);

            defender.OnDespawned += HandleDefenderDespawned;
            _activeDefenders.Add(defender);
        }

        private void HandleDefenderDespawned(DefenderFacade defender)
        {
            defender.OnDespawned -= HandleDefenderDespawned;
            _activeDefenders.Remove(defender);
        }

        // Вместо OnDestroy теперь используем метод для очистки, 
        // который можно будет вызывать при удалении башни.
        public void Cleanup()
        {
            for (int i = _activeDefenders.Count - 1; i >= 0; i--)
            {
                var defender = _activeDefenders[i];
                if (defender != null)
                {
                    defender.OnDespawned -= HandleDefenderDespawned;
                    
                    if (defender.gameObject.activeInHierarchy)
                    {
                        defender.Despawn();
                    }
                }
            }
            _activeDefenders.Clear();
        }
    }
}