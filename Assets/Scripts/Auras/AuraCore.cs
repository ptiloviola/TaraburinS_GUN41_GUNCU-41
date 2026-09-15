using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Gameplay.Auras.Data;
using Gameplay.Enemies;

namespace Gameplay.Auras
{
    public class AuraCore
    {
        private AuraSetup _setup;
        private Vector3 _centerPosition;
        private LayerMask _enemyMask; // Маска слоя врагов для оптимизации физики
        private CancellationTokenSource _cts;

        // События для Визуализатора (View)
        public event Action<float> OnAuraStarted; // Передаем радиус
        public event Action OnAuraFinished;

        public void StartAura(AuraSetup setup, Vector3 position, LayerMask enemyMask)
        {
            _setup = setup;
            _centerPosition = position;
            _enemyMask = enemyMask;

            StopAura(); // Защита от двойного запуска
            _cts = new CancellationTokenSource();

            OnAuraStarted?.Invoke(_setup.Radius);
            
            // Запускаем асинхронный процесс без блокировки основного потока
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

                    // Ждем время одного тика, учитывая timeScale (паузу)
                    await UniTask.Delay(TimeSpan.FromSeconds(_setup.TickRate), 
                        ignoreTimeScale: false, 
                        cancellationToken: token);

                    elapsed += _setup.TickRate;
                }
            }
            catch (OperationCanceledException)
            {
                // Тихо гасим исключение при отмене (смерть зоны до истечения таймера)
            }
            finally
            {
                // Когда время вышло или лужу уничтожили - сообщаем об этом
                OnAuraFinished?.Invoke();
            }
        }

        private void ApplyEffectsToTargetsInRadius()
        {
            // Оптимизированный поиск физики (без аллокаций, если использовать NonAlloc, 
            // но для простоты читаемости пока берем обычный OverlapSphere)
            Collider[] hits = Physics.OverlapSphere(_centerPosition, _setup.Radius, _enemyMask);

            foreach (Collider hit in hits)
            {
                if (hit.TryGetComponent(out EnemyFacade enemy) && !enemy.IsDead)
                {
                    ApplyStatusesToEnemy(enemy);
                }
            }
        }

        private void ApplyStatusesToEnemy(EnemyFacade enemy)
        {
            if (_setup.StatusEffects == null) return;

            foreach (var statusConfig in _setup.StatusEffects)
            {
                // Фабрика статуса создает "кубик" (FreezeStatus или PoisonStatus)
                var effect = statusConfig.CreateEffect();
                
                // Враг забирает статус и сам считает свои резисты
                enemy.StatusController.AddStatus(effect);
            }
        }
    }
}