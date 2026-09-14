using System;
using System.Collections.Generic;
using Gameplay.Enemies.Data;
using UnityEngine;
using Gameplay.Core.Statuses;

namespace Gameplay.Enemies.Statuses
{
    public class EnemyStatusController
    {
        private readonly EnemyFacade _enemy;
        private readonly List<IStatusEffect> _activeEffects = new List<IStatusEffect>();

        public float SpeedMultiplier { get; private set; } = 1f;
        public float DamageTakenMultiplier { get; private set; } = 1f;

        public event Action<IStatusEffect> OnStatusAdded;
        public event Action<IStatusEffect> OnStatusRemoved;

        public EnemyStatusController(EnemyFacade enemy)
        {
            _enemy = enemy;
        }

        public void AddStatus(IStatusEffect effect)
        {
            float resistMultiplier = _enemy.Config.GetResistMultiplier(effect.Type);

            if (resistMultiplier <= 0f)
            {
#if UNITY_EDITOR
                Debug.Log($"<color=grey>[Status] Враг {_enemy.gameObject.name} иммунен к {effect.Type}. Статус {effect.Id} отклонен.</color>");
#endif
                return;
            }

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