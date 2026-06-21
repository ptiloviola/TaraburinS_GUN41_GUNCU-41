using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Gameplay.Towers.Data.Modules;
using Gameplay.Units;


namespace Gameplay.Towers.Behaviors
{
    public class BarracksBehavior : MonoBehaviour
    {
        private TowerFacade _facade;
        private BarracksModuleDescriptor _module;
        private DefenderFacade.Pool _defenderPool;

        // Заменяем инжект пула на инжект самого DI-контейнера
        private DiContainer _container;
        // Список живых бойцов, привязанных к этой казарме
        private List<DefenderFacade> _activeDefenders = new List<DefenderFacade>();

        private float _respawnTimer;
        private Vector3 _rallyPoint;

        // Zenject внедрит нам Пул защитников
        [Inject]
        public void Construct(DiContainer container)
        {
            _container = container;
        }

        public void Initialize(TowerFacade facade)
        {
            _facade = facade;
            _module = _facade.GetCurrentStats().Barracks;
            if (_module == null || _module.DefenderData == null)
            {
                Debug.LogError($"[BarracksBehavior] На башне {_facade.name} нет модуля Barracks, но скрипт висит!");
                return;
            }

            // НОВОЕ: Динамически получаем нужный пул по ID из конфига!
            _defenderPool = _container.ResolveId<DefenderFacade.Pool>(_module.DefenderData.DefenderId);

            // Устанавливаем точку сбора. Пока это просто точка перед башней.
            // В будущем мы сделаем так, чтобы игрок мог кликать и менять ее позицию!
            _rallyPoint = transform.position + transform.forward * (_module.RallyPointRadius * 0.5f);
            // Сбрасываем таймер
            _respawnTimer = _module.RespawnCooldown;
            // Если башня проапгрейдилась (Initialize вызвался повторно), 
            // нам нужно обновить статы всем уже живым защитникам!
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
            // Чистим список от "мертвых" или вернувшихся в пул солдат
            _activeDefenders.RemoveAll(d => d == null || !d.gameObject.activeInHierarchy);
            // Если гарнизон не полон — начинаем подготовку нового бойца
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
            // 1. Просим Zenject выдать нам свободного человечка из пула
            var defender = _defenderPool.Spawn();
            // 2. Ставим его у основания башни
            defender.transform.position = transform.position;
            // 3. Накатываем на него конфиг (скорость и т.д.)
            defender.InitConfig(_module.DefenderData);
            // 4. Немного рандомизируем позицию точки сбора, чтобы они не слипались в одну кучу
            Vector3 randomOffset = Random.insideUnitSphere * 1.5f;
            randomOffset.y = 0; // Запрещаем им летать или проваливаться под землю
            
            // 5. Отдаем приказ бежать!
            defender.SendToRallyPoint(_rallyPoint + randomOffset);

            // 6. Записываем в журнал учета
            _activeDefenders.Add(defender);
            
            Debug.Log($"[BarracksBehavior] Из казармы выбежал защитник! В строю: {_activeDefenders.Count}/{_module.MaxDefenders}");
        }

        // Если башню продают или уничтожают, нужно убрать всех ее защитников с карты
        private void OnDestroy()
        {
            foreach (var defender in _activeDefenders)
            {
                if (defender != null && defender.gameObject.activeInHierarchy)
                {
                    defender.Despawn();
                }
            }
            _activeDefenders.Clear();
        }





    }

}
