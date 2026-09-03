using System;
using System.Collections.Generic;
using Gameplay.Enemies.Data;
using UnityEngine;

namespace Gameplay.Enemies.Statuses
{
    public class EnemyStatusController
    {
        private readonly EnemyFacade _enemy;
        private readonly List<IStatusEffect> _activeEffects = new List<IStatusEffect>();

        // Публичные множители для других систем (Начинаются с 1.0 = 100%)
        public float SpeedMultiplier { get; private set; } = 1f;
        public float DamageTakenMultiplier { get; private set; } = 1f;

        // События для отвязанного UI и визуала
        public event Action<IStatusEffect> OnStatusAdded;
        public event Action<IStatusEffect> OnStatusRemoved;

        public EnemyStatusController(EnemyFacade enemy)
        {
            _enemy = enemy;
        }

        public void AddStatus(IStatusEffect effect)
        {
            // 1. Узнаем множитель из конфига врага на основе типа эффекта
            float resistMultiplier = effect.Type switch
            {
                StatusType.Control => _enemy.Config.StatusResist.ControlMultiplier,
                StatusType.DamageOverTime => _enemy.Config.StatusResist.DoTMultiplier,
                StatusType.Debuff => _enemy.Config.StatusResist.DebuffMultiplier,
                _ => 1f
            };

            // 2. Если иммунитет (0) — даже не добавляем в список
            if (resistMultiplier <= 0f)
            {
                Debug.Log($"<color=grey>[Status] Враг {_enemy.gameObject.name} иммунен к {effect.Type}. Статус отклонен.</color>");
                return;
            }

            // 3. Срезаем время и добавляем
            effect.ApplyResistance(resistMultiplier);
            
            _activeEffects.Add(effect);
            effect.OnApply(_enemy);
            RecalculateMultipliers();
            OnStatusAdded?.Invoke(effect);
        }

        public void Tick(float deltaTime)
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];
                effect.Tick(deltaTime);

                if (effect.IsFinished)
                {
                    RemoveStatus(effect);
                }
            }
        }

        private void RemoveStatus(IStatusEffect effect)
        {
            effect.OnRemove();
            _activeEffects.Remove(effect);
            RecalculateMultipliers();
            OnStatusRemoved?.Invoke(effect);
        }

        public void Cleanup()
        {
            foreach (var effect in _activeEffects)
            {
                effect.OnRemove();
                OnStatusRemoved?.Invoke(effect);
            }
            _activeEffects.Clear();
            RecalculateMultipliers();
        }

        private void RecalculateMultipliers()
        {
            SpeedMultiplier = 1f;
            DamageTakenMultiplier = 1f;

            foreach (var effect in _activeEffects)
            {
                SpeedMultiplier *= effect.SpeedModifier;
                DamageTakenMultiplier *= effect.DamageTakenModifier;
            }
        }
    }
}