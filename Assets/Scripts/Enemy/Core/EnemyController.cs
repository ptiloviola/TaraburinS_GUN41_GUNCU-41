using UnityEngine;
using Infrastructure.Interfaces;
using Enemy.Config;
namespace Enemy.Core
{
    public class EnemyController : MonoBehaviour, IDamageable, IVisibleTarget
    {
        [Header("Settings")]
        [SerializeField] private EnemyConfig _config;

        private IMover _mover;
        private IEnemyView _view;
        private IHealth _health;

        private float _wanderTimer;

        private void Awake()
        {
            // Сборка зависимостей через интерфейсы
            _mover = GetComponent<IMover>();
            _view = GetComponent<IEnemyView>();
            _health = GetComponent<IHealth>();

            // Если на снеговике висят наши скрипты, инициализируем их специфичные данные
            if (_mover is Movement.NavMeshMover navMover) navMover.Initialize(_config.moveSpeed, _config.baseOffset);
            if (_view is Visuals.SnowmanTweenView tweenView) tweenView.Initialize(_config);
            if (_health is EnemyHealth healthComponent) healthComponent.Initialize(_config.maxHealth);
        }

        private void OnEnable()
        {
            _health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            _health.OnDeath -= HandleDeath;
        }

        private void Update()
        {
            // Логика ИИ блуждания (В будущем это можно вынести в IAIBenaviour)
            _wanderTimer += Time.deltaTime;
            if (_wanderTimer >= _config.wanderTimer)
            {
                Vector3 randomDirection = Random.insideUnitSphere * _config.wanderRadius + transform.position;
                _mover.SetDestination(randomDirection);
                _wanderTimer = 0f;
            }

            // Синхронизируем движение и графику
            _view.UpdateMoveAnimation(_mover.CurrentSpeed, transform.eulerAngles.y);
        }

        // Метод, который дергает радар игрока
        public void SetVisibility(bool isVisible) => _view.SetVisibility(isVisible);

        // Метод, который дергает пуля игрока
        public void TakeDamage(int amount, Vector3 hitPoint)
        {
            _health.TakeDamage(amount, hitPoint);
            _view.PlayHitReaction(); // Сразу запускаем визуал шлепка
        }

        private void HandleDeath()
        {
            _mover.Stop();
            _view.PlayDeathEffect();
            
            // Выключаем коллайдеры, чтобы мертвеца нельзя было расстрелять снова
            foreach (var col in GetComponentsInChildren<Collider>()) col.enabled = false;

            // Полное удаление объекта через полсекунды (после завершения твина исчезновения)
            Destroy(gameObject, 0.5f);
        }
    }
}