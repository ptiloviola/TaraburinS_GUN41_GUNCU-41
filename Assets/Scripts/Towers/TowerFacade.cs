using Gameplay.Towers.Data;
using UnityEngine;

namespace Gameplay.Towers
{
    public class TowerFacade : MonoBehaviour
    {
        [SerializeField] private TowerConfig _config;
        
        public int CurrentLevel { get; private set; }
        public TowerConfig Config => _config;

        private ITowerBehavior[] _behaviors; // Массив всех модулей башни


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
            CurrentLevel = 0;
            
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
            // Здесь в будущем добавим перерисовку VisualPrefab
        }
    }
}