using System;
using System.Collections.Generic;
using Gameplay.Enemies.Data;
using UnityEngine;
using Gameplay.Combat.Statuses;

namespace Gameplay.Enemies.Statuses
{
    public class EnemyStatusController
    {
        private readonly GameObject _enemy;
        private readonly List<IStatusEffect> _activeEffects = new List<IStatusEffect>();
        
        private Func<StatusType, float> _getResistanceMultiplier; 

        public float SpeedMultiplier { get; private set; } = 1f;
        public float DamageTakenMultiplier { get; private set; } = 1f;

        public event Action<IStatusEffect> OnStatusAdded;
        public event Action<IStatusEffect> OnStatusRemoved;

        public EnemyStatusController(GameObject enemy)
        {
            _enemy = enemy;
        }


        public void Initialize(Func<StatusType, float> getResistanceMultiplier)
        {
            _getResistanceMultiplier = getResistanceMultiplier;
        }

        public void AddStatus(IStatusEffect effect)
        {
            if (_getResistanceMultiplier == null) return;


            float resistMultiplier = _getResistanceMultiplier(effect.Type);

            if (resistMultiplier <= 0f)
            {
                Gameplay.Tools.GameLogger.Log($"<color=grey>[Status] Враг {_enemy.gameObject.name} иммунен к {effect.Type}. Статус {effect.Id} отклонен.</color>");
                return;
            }
            var existingEffect = _activeEffects.Find(e => e.Id == effect.Id);
            if (existingEffect != null)
            {
                RemoveStatus(existingEffect);
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