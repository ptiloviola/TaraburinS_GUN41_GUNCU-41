using Gameplay.Towers.Data;
using UnityEngine;
using System;

namespace Gameplay.Towers
{
    public class TowerFacade : MonoBehaviour
    {
        [SerializeField] private TowerConfig _config;
        
        public int CurrentLevel { get; private set; }
        public TowerConfig Config => _config;

        // НОВОЕ: Перенесли координату клетки прямо сюда!
        public Vector2Int GridPosition { get; private set; }

        private ITowerBehavior[] _behaviors; // Массив всех модулей башни
        // НОВОЕ: Событие для обновления UI-панели магазина
        public event Action OnLevelChanged;


        // Добавляем Unity-метод Start для автономного дебага
        private void Start()
        {
            // Если массив поведений еще не создан, значит Initialize не вызывался извне.
            // Инициализируем башню самостоятельно дефолтным конфигом из инспектора.
            if (_behaviors == null)
            {
                if (_config != null)
                {
                    Initialize(_config, Vector2Int.zero);
                }
                else
                {
                    Debug.LogError($"[TowerFacade] На объекте {name} нет конфигурации TowerConfig!");
                }
            }
        }

        public void Initialize(TowerConfig config, Vector2Int gridPos)
        {
            _config = config;
            GridPosition = gridPos;
            
            Debug.Log($"<color=cyan>[TowerFacade] Начинаем сборку башни {_config.DisplayName} на клетке {gridPos}</color>");

            // РЕНТГЕН: Получаем вообще ВСЕ скрипты на клоне (даже выключенные)
            var allScripts = GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var script in allScripts)
            {
                if (script != null)
                {
                    // Спрашиваем C#: "Считаешь ли ты этот скрипт модулем башни?"
                    bool isBehavior = script is ITowerBehavior;
                    Debug.Log($"[TowerFacade-Рентген] Нашел скрипт: <color=yellow>{script.GetType().Name}</color>. Является ли он ITowerBehavior? <b>{isBehavior}</b>");
                }
            }

            // Ищем модули, включив поиск по неактивным объектам (true)
            _behaviors = GetComponentsInChildren<ITowerBehavior>(true);

            Debug.Log($"<color=cyan>[TowerFacade] Итог: Найдено модулей: {_behaviors.Length}</color>");

            foreach (var behavior in _behaviors)
            {
                Debug.Log($"<color=cyan>[TowerFacade] Запускаем модуль: {behavior.GetType().Name}</color>");
                behavior.Initialize(this);
            }
        }

        private void Update()
        {
            if (_behaviors == null) return;

            // Каждый кадр заставляем работать только те модули, которые висят на башне
            foreach (var behavior in _behaviors)
            {
                behavior.Tick();
            }
        }

        public TowerLevelData GetCurrentStats() => _config.Levels[CurrentLevel];
        public bool CanUpgrade() => CurrentLevel < _config.MaxLevel;

        public void Upgrade()
        {
            if (!CanUpgrade()) return;
            CurrentLevel++;
            // НОВОЕ: Заставляем все модули перечитать статы из конфига!
            // Так как CurrentLevel увеличился, GetCurrentStats() теперь вернет новые данные.
            foreach (var behavior in _behaviors)
            {
                behavior.Initialize(this); 
            }

            // Оповещаем UI, что уровень изменился
            OnLevelChanged?.Invoke();
            
            Debug.Log($"<color=green>[TowerFacade] {_config.DisplayName} улучшена до уровня {CurrentLevel + 1}!</color>");
            // Здесь в будущем добавим перерисовку VisualPrefab
        }
    }
}