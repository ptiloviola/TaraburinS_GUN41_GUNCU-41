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
                    Initialize(_config);
                }
                else
                {
                    Debug.LogError($"[TowerFacade] На объекте {name} нет конфигурации TowerConfig!");
                }
            }
        }

        public void Initialize(TowerConfig config)
        {
            _config = config;
            // CurrentLevel = 0; // ВНИМАНИЕ: Если ты будешь загружать сохранения, уровень нужно будет брать оттуда
            
            // МАГИЯ КОМПОЗИЦИИ: 
            // Ищем все скрипты на этом префабе, которые реализуют ITowerBehavior
            _behaviors = GetComponentsInChildren<ITowerBehavior>();

            // Инициализируем каждый модуль
            foreach (var behavior in _behaviors)
            {
                behavior.Initialize(this);
            }
            
            Debug.Log($"<color=orange>[TowerFacade] Башня {_config.DisplayName} успешно собрана автономно. Модулей: {_behaviors.Length}</color>");
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