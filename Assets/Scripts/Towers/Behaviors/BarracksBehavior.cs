using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Gameplay.Towers.Data.Modules;
using Gameplay.Units;


namespace Gameplay.Towers.Behaviors
{
    public class BarracksBehavior : MonoBehaviour, ITowerBehavior
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
            Debug.Log($"<color=yellow>[BarracksBehavior] Zenject внедрил контейнер в {gameObject.name}</color>");
        }

        public void Initialize(TowerFacade facade)
        {
            Debug.Log($"<color=yellow>[BarracksBehavior] Старт Initialize на {gameObject.name}</color>");
            _facade = facade;
            _module = _facade.GetCurrentStats().Barracks;
            if (_module == null || _module.DefenderData == null)
            {
                Debug.LogError($"[BarracksBehavior] На башне {_facade.name} нет модуля Barracks, но скрипт висит!");
                return;
            }

            if (_module.DefenderData == null)
            {
                Debug.LogError($"[BarracksBehavior] ОШИБКА: Не назначен DefenderData в конфиге!");
                return;
            }

            Debug.Log($"[BarracksBehavior] Пытаемся получить пул с ID: {_module.DefenderData.DefenderId}");


            // НОВОЕ: Динамически получаем нужный пул по ID из конфига!
            _defenderPool = _container.ResolveId<DefenderFacade.Pool>(_module.DefenderData.DefenderId);

            // Устанавливаем точку сбора. Пока это просто точка перед башней.
            // В будущем мы сделаем так, чтобы игрок мог кликать и менять ее позицию!
            _rallyPoint = transform.position + transform.forward * (_module.RallyPointRadius * 0.5f);
            // Сбрасываем таймер
            _respawnTimer = _module.RespawnCooldown;
            Debug.Log($"<color=green>[BarracksBehavior] Инициализация успешна! Таймер: {_respawnTimer}</color>");
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
            Debug.Log("<color=magenta>[BarracksBehavior] ПОПЫТКА СПАВНА!</color>");
            // 1. Просим Zenject выдать нам свободного человечка из пула
            var defender = _defenderPool.Spawn();
            // 1. Ищем безопасную точку на NavMesh (в радиусе 3 метров от башни)
            Vector3 desiredSpawnPos = transform.position + transform.forward * 2f;
            if (UnityEngine.AI.NavMesh.SamplePosition(desiredSpawnPos, out UnityEngine.AI.NavMeshHit hit, 3f, UnityEngine.AI.NavMesh.AllAreas))
            {
                Debug.Log("<color=magenta>[BarracksBehavior] ПОПЫТКА defender.WarpTo(hit.position)</color>");
                // 2. Безопасно телепортируем агента
                defender.WarpTo(hit.position);
            }
            else
            {
                Debug.Log("<color=magenta>[BarracksBehavior] ПОПЫТКА defender.WarpTo(transform.position</color>");
                // Если рядом нет NavMesh (например, башня висит в воздухе), кидаем в центр
                defender.WarpTo(transform.position); 
            }
            // 3. Накатываем на него конфиг (скорость и т.д.)
            defender.InitConfig(_module.DefenderData);
            // 4. Немного рандомизируем позицию точки сбора, чтобы они не слипались в одну кучу
            Vector3 randomOffset = Random.insideUnitSphere * 1.5f;
            randomOffset.y = 0; // Запрещаем им летать или проваливаться под землю
            
            // 5. Отдаем приказ бежать!
            defender.SendToRallyPoint(_rallyPoint + randomOffset);

            // 6. Записываем в журнал учета
            _activeDefenders.Add(defender);
            
            Debug.Log($"<color=magenta>[BarracksBehavior] ЗАЩИТНИК ЗАСПАВНЕН! В строю: {_activeDefenders.Count}</color>");
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
