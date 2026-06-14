using UnityEngine;
using Gameplay.Enemies;
using Infrastructure.Signals; // Подключаем наши сигналы
using Zenject;
using Gameplay.Grid; // Подключаем доступ к сетке



namespace Gameplay.Base
{
    // НОВОЕ: Перечисление режимы работы здоровья
    public enum BaseHealthMode { Global, Individual }
    // Требуем, чтобы на объекте обязательно был коллайдер
    [RequireComponent(typeof(Collider))]
    public class BaseCore : MonoBehaviour
    {
        [Header("Настройки визуала")]
        [Tooltip("Если база уходит под землю, увеличь этот параметр в Инспекторе ПРЕФАБА")]
        [SerializeField] private float _verticalOffset = 0.5f;

        [Header("Настройки Базы")]
        public string BaseId = "MainBase"; // НОВОЕ ПОЛЕ


        [Header("Настройки Здоровья")]
        [Tooltip("Global = снимает общие жизни игрока. Individual = у базы свои ХП.")]
        [SerializeField] private BaseHealthMode _healthMode = BaseHealthMode.Global;

        // Это поле работает только если выбран режим Individual
        [SerializeField] private int _individualLives = 20;


        private SignalBus _signalBus;
        private BaseRegistry _baseRegistry; // НОВОЕ: Ссылка на реестр баз
        private IGridService _gridService;

        private PlayerHealthService _playerHealthService; // НОВОЕ

        // Публичное свойство, чтобы GridGenerator мог прочитать, на сколько поднять базу
        public float VerticalOffset => _verticalOffset;

        // Внедряем SignalBus через Zenject
        [Inject]
        public void Construct(SignalBus signalBus, BaseRegistry baseRegistry, 
            PlayerHealthService playerHealthService)
        {
            _signalBus = signalBus;
            _baseRegistry = baseRegistry;
            _playerHealthService = playerHealthService;
        }

        // НОВОЕ: Автоматическая регистрация при спавне
        // ИСПРАВЛЕНИЕ: Меняем OnEnable/OnDisable на Start/OnDestroy
        private void Start()
        {
            // Знак '?' спасает от ошибки, если база на сцене до инициализации Zenject
            _baseRegistry?.Register(this);
        }

        private void OnDestroy()
        {
            _baseRegistry?.Unregister(this);
        }


        private void OnTriggerEnter(Collider other)
        {

            // Лог 1: Сработало ли вообще физическое касание?
            Debug.Log($"<color=cyan>[BaseCore] Что-то коснулось базы! Имя объекта: {other.gameObject.name}</color>");
            /// Лог 2: Пытаемся найти наш фасад на объекте
            // Ищем фасад на самом объекте ИЛИ поднимаемся вверх до корня префаба
            EnemyFacade enemy = other.GetComponentInParent<EnemyFacade>();

            if (enemy != null)
            {
                Debug.Log($"<color=green>[BaseCore] Нашли EnemyFacade на {enemy.gameObject.name}! Уничтожаем.</color>");
                // Отнимаем жизнь и проверяем поражение
                TakeDamage(1);
                enemy.Despawn();
            }
            else
            {
                // Лог 3: Касание было, но нужного скрипта нет ни тут, ни у родителей
                Debug.LogWarning($"<color=red>[BaseCore] В базу врезалось что-то без EnemyFacade: {other.gameObject.name}!</color>");
            }
        }

        private void TakeDamage(int amount)
        {
            // --- НОВАЯ УМНАЯ ЛОГИКА ---
            if (_healthMode == BaseHealthMode.Global)
            {
                // Передаем урон Глобальному менеджеру (UI не будет прыгать!)
                _playerHealthService.TakeGlobalDamage(amount);
            }
            else
            {
                // Режим независимой базы
                _individualLives -= amount;
                Debug.Log($"<color=orange>[BaseCore] База {BaseId} получила урон. Осталось личных жизней: {_individualLives}</color>");
                
                // Задел на будущее: 
                // _signalBus.Fire(new SignalSpecificBaseDamaged { BaseId = this.BaseId, Lives = _individualLives });
                
                if (_individualLives <= 0)
                {
                    Debug.Log($"<color=red>[BaseCore] База {BaseId} УНИЧТОЖЕНА!</color>");
                    // Здесь будет логика взрыва конкретной базы и удаления её с карты
                    Destroy(gameObject); 
                }
            }
        }

        // НОВОЕ: Паттерн Фабрики для создания префабов базы через Zenject
        public class Factory : PlaceholderFactory<BaseCore> { }
    }
}