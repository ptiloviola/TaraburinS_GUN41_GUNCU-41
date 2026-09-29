using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Gameplay.Auras.Data;
using Gameplay.Enemies;
using Gameplay.Combat;

namespace Gameplay.Auras
{
    public class AuraCore
    {
        private AuraSetup _setup;
        private Vector3 _centerPosition;
        private LayerMask _enemyMask;
        private TargetType _allowedTargets;
        private CancellationTokenSource _cts;

        public event Action<float> OnAuraStarted;
        public event Action OnAuraFinished;

        public void StartAura(AuraSetup setup, Vector3 position, LayerMask enemyMask, TargetType allowedTargets)
        {
            _setup = setup;
            _centerPosition = position;
            _enemyMask = enemyMask;
            _allowedTargets = allowedTargets;

            StopAura();
            _cts = new CancellationTokenSource();

            OnAuraStarted?.Invoke(_setup.Radius);
            
            ProcessAuraLifetimeAsync(_cts.Token).Forget();
        }

        public void StopAura()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }

        private async UniTaskVoid ProcessAuraLifetimeAsync(CancellationToken token)
        {
            float elapsed = 0f;

            try
            {
                while (elapsed < _setup.Duration)
                {
                    ApplyEffectsToTargetsInRadius();

                    await UniTask.Delay(TimeSpan.FromSeconds(_setup.TickRate), 
                        ignoreTimeScale: false, 
                        cancellationToken: token);

                    elapsed += _setup.TickRate;
                }
            }
            catch (OperationCanceledException)
            {
                
            }
            finally
            {
                OnAuraFinished?.Invoke();
            }
        }

        private void ApplyEffectsToTargetsInRadius()
        {
            Collider[] hits = Physics.OverlapSphere(_centerPosition, _setup.Radius, _enemyMask);

            foreach (Collider hit in hits)
            {
                if (hit.TryGetComponent(out EnemyFacade enemy) && !enemy.IsDead)
                {
                    if ((_allowedTargets & enemy.TargetType) != 0) 
                    {
                        if (hit.TryGetComponent(out IDamageable damageable))
                        {
                            damageable.TakeDamage(_setup.DamagePayload);
                        }
                        ApplyStatusesToEnemy(enemy);
                    }
                }
            }
        }

        private void ApplyStatusesToEnemy(EnemyFacade enemy)
        {
            if (_setup.StatusEffects == null) return;

            foreach (var statusConfig in _setup.StatusEffects)
            {
                var effect = statusConfig.CreateEffect();
                
                enemy.StatusController.AddStatus(effect);
            }
        }
    }
}