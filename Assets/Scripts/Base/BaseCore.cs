using UnityEngine;
using Gameplay.Enemies;
using Gameplay.Infrastructure.Signals;
using Zenject;

namespace Gameplay.Base
{
    public enum BaseHealthMode { Global, Individual }

    [RequireComponent(typeof(Collider))]
    public class BaseCore : MonoBehaviour
    {
        [Header("Настройки визуала")]
        [SerializeField] private float _verticalOffset = 0.5f;

        [Header("Настройки Базы")]
        [SerializeField] private string _baseId = "MainBase";

        [Header("Настройки Здоровья")]
        [SerializeField] private BaseHealthMode _healthMode = BaseHealthMode.Global;
        [SerializeField] private int _individualLives = 20;

        private SignalBus _signalBus;
        private BaseRegistry _baseRegistry;
        private PlayerHealthService _playerHealthService;

        private const int DefaultEnemyDamage = 1;

        public float VerticalOffset => _verticalOffset;
        public string BaseId
        {
            get => _baseId;
            set => _baseId = value;
        }

        [Inject]
        public void Construct(SignalBus signalBus, BaseRegistry baseRegistry, PlayerHealthService playerHealthService)
        {
            _signalBus = signalBus;
            _baseRegistry = baseRegistry;
            _playerHealthService = playerHealthService;
        }

        private void Start()
        {
            _baseRegistry?.Register(this);
        }

        private void OnDestroy()
        {
            _baseRegistry?.Unregister(this);
        }


        public void TakeDamage(int amount)
        {
            if (_healthMode == BaseHealthMode.Global)
            {
                _playerHealthService.TakeGlobalDamage(amount);
            }
            else
            {
                _individualLives -= amount;
#if UNITY_EDITOR
                Gameplay.Tools.GameLogger.Log($"<color=orange>[BaseCore] База {_baseId} получила урон. Осталось личных жизней: {_individualLives}</color>");
#endif
                
                if (_individualLives <= 0)
                {
#if UNITY_EDITOR
                    Gameplay.Tools.GameLogger.Log($"<color=red>[BaseCore] База {_baseId} УНИЧТОЖЕНА!</color>");
#endif
                    Destroy(gameObject); 
                }
            }
        }

        public class Factory : PlaceholderFactory<BaseCore> { }
    }
}