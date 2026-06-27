using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Gameplay.Towers.Data.Modules;
using Gameplay.Units;
using Gameplay.Towers.Factories; // Подключаем пространство имен нашей новой фабрики

namespace Gameplay.Towers.Behaviors
{
    public class BarracksBehavior : MonoBehaviour, ITowerBehavior
    {
        private TowerFacade _facade;
        private BarracksModuleDescriptor _module;
        
        // Вместо DiContainer теперь инжектим конкретную фабрику
        private DefenderFactory _defenderFactory;
        
        private List<DefenderFacade> _activeDefenders = new List<DefenderFacade>();
        private float _respawnTimer;
        private Vector3 _rallyPoint;

        [Inject]
        public void Construct(DefenderFactory defenderFactory)
        {
            _defenderFactory = defenderFactory;
        }

        public void Initialize(TowerFacade facade)
        {
            _facade = facade;
            _module = _facade.GetCurrentStats().Barracks;
            
            if (_module == null || _module.DefenderData == null) return;

            _rallyPoint = transform.position + transform.forward * (_module.RallyPointRadius * 0.5f);
            _respawnTimer = _module.RespawnCooldown;

            foreach (var defender in _activeDefenders)
            {
                if (defender != null && defender.gameObject.activeInHierarchy)
                {
                    defender.InitConfig(_module.DefenderData);
                }
            }
        }

        public void Tick()
        {
            if (_module == null) return;
            
            // ЛИКВИДИРОВАНО: _activeDefenders.RemoveAll(...) - больше никаких просадок CPU!

            if (_activeDefenders.Count < _module.MaxDefenders)
            {
                _respawnTimer -= Time.deltaTime;
                if (_respawnTimer <= 0f)
                {
                    SpawnDefender();
                    _respawnTimer = _module.RespawnCooldown;
                }
            }
        }

        private void SpawnDefender()
        {
            // 1. Делегируем создание фабрике (никакого ResolveId прямо в классе!)
            var defender = _defenderFactory.Create(_module.DefenderData.DefenderId);

            Vector3 desiredSpawnPos = transform.position + transform.forward * 2f;
            if (UnityEngine.AI.NavMesh.SamplePosition(desiredSpawnPos, out UnityEngine.AI.NavMeshHit hit, 3f, UnityEngine.AI.NavMesh.AllAreas))
            {
                defender.WarpTo(hit.position);
            }
            else
            {
                defender.WarpTo(transform.position); 
            }

            defender.InitConfig(_module.DefenderData);

            // ИСПРАВЛЕНИЕ МАТЕМАТИКИ: Используем 2D круг для плоской поверхности, чтобы точки распределялись равномерно
            Vector2 rnd = Random.insideUnitCircle * 1.5f;
            Vector3 randomOffset = new Vector3(rnd.x, 0, rnd.y);
            
            defender.SendToRallyPoint(_rallyPoint + randomOffset);

            // 2. ПОДПИСКА НА СОБЫТИЕ: Слушаем, когда этот конкретный защитник умрет
            defender.OnDespawned += HandleDefenderDespawned;

            _activeDefenders.Add(defender);
        }

        // 3. ОБРАБОТЧИК СОБЫТИЯ
        private void HandleDefenderDespawned(DefenderFacade defender)
        {
            // Всегда отписываемся от событий, чтобы Garbage Collector мог спокойно удалить объект
            defender.OnDespawned -= HandleDefenderDespawned;
            
            // Удаляем конкретного бойца из списка
            _activeDefenders.Remove(defender);
        }

        private void OnDestroy()
        {
            // Чтобы безопасно очистить список и вызвать события, 
            // идем по нему с конца (reverse for-loop)
            for (int i = _activeDefenders.Count - 1; i >= 0; i--)
            {
                var defender = _activeDefenders[i];
                if (defender != null)
                {
                    // Отписываемся, чтобы Despawn() не попытался снова удалить его из списка и не сломал цикл
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