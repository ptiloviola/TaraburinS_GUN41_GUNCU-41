using Gameplay.Towers.Data;
using UnityEngine;
using Gameplay.Towers.Data.Modules;

namespace Gameplay.Towers.Behaviors
{
    // Модуль является MonoBehaviour, значит мы можем легко повесить его на префаб
    // и привязать к нему точку вылета снаряда (FirePoint) прямо в инспекторе!
    public class AttackBehavior : MonoBehaviour, ITowerBehavior
    {
        [Header("Ссылки")]
        [SerializeField] private Transform _firePoint; // Откуда вылетает пуля

        // ДОБАВЛЯЕМ МАСКУ СЛОЯ ДЛЯ ВРАГОВ
        [SerializeField] private LayerMask _enemyLayerMask;
        
        private TowerFacade _facade;
        private float _cooldownTimer;

        public void Initialize(TowerFacade facade)
        {
            _facade = facade;
            _cooldownTimer = 0f;
            // Если точку вылета не назначили, используем центр самой башни
            if (_firePoint == null)
            {
                _firePoint = transform;
            }
        }

        public void Tick()
        {
            AttackStats stats = _facade.GetCurrentStats().Attack;
            
            if (stats == null) return;

            _cooldownTimer -= Time.deltaTime;
            
            if (_cooldownTimer <= 0f)
            {
                ExecuteTestShot(stats);
                _cooldownTimer = stats.Cooldown; // Сброс таймера перезарядки
            }
        }

        private void ExecuteTestShot(AttackStats stats)
        {
            // Пускаем физический луч вперед
            Vector3 direction = _firePoint.forward;
            Debug.Log($"<color=red>[AttackBehavior] Башня '{_facade.Config.DisplayName}' делает выстрел вперед!</color>");
            
            // Визуализируем луч в окне Scene (видно только во время воспроизведения при включенных Gizmos)
            Debug.DrawRay(_firePoint.position, direction * stats.Range, Color.red, 0.2f);

            

            // ТЕПЕРЬ ЛУЧ ИЩЕТ ТОЛЬКО ВРАГОВ (используем _enemyLayerMask)
            if (Physics.Raycast(_firePoint.position, direction, out RaycastHit hit, stats.Range, _enemyLayerMask))
            {
                Debug.Log($"<color=yellow>[AttackBehavior] ПОПАДАНИЕ! Луч пробил врага: {hit.collider.name}</color>");
            }
        }


    }
}