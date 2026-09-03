using UnityEngine;
using Gameplay.Towers.Data.Modules;
using Gameplay.Towers.Behaviors.Weapons;
// НОВОЕ: Подключаем пространство имен стратегий прицеливания
using Gameplay.Towers.Behaviors.Aiming;
using Gameplay.Projectiles.Contracts;
using System;
using Gameplay.Towers.Behaviors.Targeting;
using Gameplay.Core;

namespace Gameplay.Towers.Behaviors
{
    // Модуль является MonoBehaviour, значит мы можем легко повесить его на префаб
    // и привязать к нему точку вылета снаряда (FirePoint) прямо в инспекторе!
    public class AttackBehavior : MonoBehaviour, ITowerBehavior
    {

        [Header("Прицеливание")]
        [SerializeField] private Transform _logicalRotator; // Невидимая ось вращения
        [SerializeField] private Transform _firePoint;      // Точка вылета снаряда (внутри ротатора)
        [SerializeField] private LayerMask _enemyLayerMask;
        [SerializeField] private float _turnSpeed = 10f;    // Скорость поворота

        // НОВОЕ: События, на которые сможет подписаться визуал или звук!
        public event Action OnBuildStarted;
        public event Action<Vector3> OnShotFired;

        // Добавь это в начало класса, где объявляются переменные:
        private IAttackExecutor _attackExecutor;
        private IAimStrategy _aimStrategy; // НОВОЕ: Ссылка на стратегию прицеливания
        private ITargetingStrategy _targetingStrategy; // НОВОЕ: Ссылка на радар


        private TowerFacade _facade;
        private Transform _currentTarget;
        private float _cooldownTimer;
        private AttackStats _currentStats;

        public void Initialize(TowerFacade facade)
        {
            _facade = facade;
            _cooldownTimer = 0f;
            _currentStats = _facade.GetCurrentStats().Attack;

            // Добавь это в метод Initialize():
            _attackExecutor = GetComponentInChildren<IAttackExecutor>();
            if (_attackExecutor == null)
            {
                Debug.LogError($"[AttackBehavior] На башне {gameObject.name} нет компонента IAttackExecutor (Оружия)!");
            }
            // НОВОЕ: Ищем стратегию прицеливания
            _aimStrategy = GetComponentInChildren<IAimStrategy>();
            if (_aimStrategy == null)
            {
                Debug.LogError($"[AttackBehavior] На башне {gameObject.name} нет компонента IAimStrategy (Прицеливания)!");
            }
            // Ищем радар
            _targetingStrategy = GetComponentInChildren<ITargetingStrategy>();
            if (_targetingStrategy == null) Debug.LogError($"[AttackBehavior] Нет ITargetingStrategy на {gameObject.name}");
            // Сообщаем всем подписчикам, что стройка началась
            OnBuildStarted?.Invoke();
            
        }

        public void Tick()
        {
            if (_currentStats == null || _targetingStrategy == null) return;

            _cooldownTimer -= Time.deltaTime;



            // 1. ПОИСК ЦЕЛИ (Делегируем Радару)
            if (!_targetingStrategy.IsTargetValid(_currentTarget, _logicalRotator, _currentStats.Range, _aimStrategy))
            {
                _currentTarget = _targetingStrategy.FindTarget(_logicalRotator, _currentStats.Range, _enemyLayerMask, _aimStrategy);
            }

            // 2. Если цель есть — поворачиваемся и стреляем
            if (_currentTarget != null)
            {
                AimAtTarget();

                // Стреляем, если прошла перезарядка И дуло смотрит на врага
                if (_cooldownTimer <= 0f && IsFacingTarget())
                {
                    // ИСПРАВЛЕНО: Передаем весь объект _currentStats, а не только Damage
                    ExecuteShot(_currentStats);
                    _cooldownTimer = _currentStats.Cooldown;
                }
            }
            
        }


        // НОВОЕ: Делегируем поворот стратегии
        private void AimAtTarget()
        {
            _aimStrategy?.AimAtTarget(_logicalRotator, _currentTarget, _turnSpeed);
        }

        // НОВОЕ: Делегируем проверку угла стратегии
        private bool IsFacingTarget()
        {
            if (_aimStrategy == null) return false;
            return _aimStrategy.IsFacingTarget(_logicalRotator, _currentTarget);
        }

        private void ExecuteShot(AttackStats stats)
        {
            Debug.Log($"<color=red>[AttackBehavior] Выстрел по {_currentTarget.name}!</color>");
            Debug.DrawRay(_firePoint.position, _logicalRotator.forward * 5f, Color.red, 0.2f);
            
            // ИСПРАВЛЕНО: ЗАЩИТА ОТ NULL
            if (stats.PayloadStrategy == null)
            {
                Debug.LogError($"[AttackBehavior] КРИТИЧЕСКАЯ ОШИБКА: На башне {gameObject.name} не назначен PayloadStrategy! Закиньте ScriptableObject в поле AttackStats.");
                return;
            }
            // 1. Просим конфиг собрать нам посылку
            DamagePayload damagePayload = new DamagePayload(stats.Damage, stats.Type);
            IProjectilePayload payload = stats.PayloadStrategy.CreatePayload(damagePayload);
            
            _attackExecutor?.ExecuteAttack(_currentTarget, payload, _firePoint);

            // Сообщаем всем подписчикам (визуалу/звуку), что произошел выстрел
            OnShotFired?.Invoke(_currentTarget.position);
        }

        // --- БЛОК ДЕБАГА И ВИЗУАЛИЗАЦИИ ---
        
        // Сохраняем радиус для отрисовки, так как Gizmos работает даже на паузе
        private float _debugRange = 5f; 

        private void OnDrawGizmos()
        {
            float drawRange = 5f; 
            if (Application.isPlaying && _facade != null && _facade.GetCurrentStats() != null)
            {
                drawRange = _facade.GetCurrentStats().Attack.Range;
            }

            Vector3 center = _logicalRotator != null ? _logicalRotator.position : transform.position;

            // Рисуем границы радара
            Gizmos.color = _currentTarget != null ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(center, drawRange);

            // ---> ВОТ ЭТОТ БЛОК ОТВЕЧАЕТ ЗА ЛУЧИ УГЛОВ <---
            if (Application.isPlaying && _aimStrategy != null)
            {
                _aimStrategy.DrawAimGizmo(_logicalRotator != null ? _logicalRotator : transform, drawRange);
            }
            else
            {
                // Для отрисовки прямо в редакторе (когда игра на паузе)
                GetComponent<IAimStrategy>()?.DrawAimGizmo(_logicalRotator != null ? _logicalRotator : transform, drawRange);
            }
            // ----------------------------------------------

            if (_currentTarget != null && _firePoint != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(_firePoint.position, _currentTarget.position);
            }
        }

    }
}