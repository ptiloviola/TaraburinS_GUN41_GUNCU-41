using System;
using UnityEngine;
using Zenject;
using Gameplay.Auras.Data;
using Gameplay.Auras.Visuals;
using Cysharp.Threading.Tasks;

namespace Gameplay.Auras
{
    public class LingeringAuraFacade : MonoBehaviour, IPoolable<AuraSetup, Vector3, LayerMask, IMemoryPool>, IDisposable
    {
        [SerializeField] private AuraZoneVisualizer _visualizer;
        
        private AuraCore _core;
        private IMemoryPool _pool;
        private bool _isDespawning;

        public AuraCore Core => _core;

        [Inject]
        public void Construct()
        {
            _core = new AuraCore();
        }

        private void OnEnable()
        {
            _core.OnAuraFinished += HandleAuraFinished;
        }

        private void OnDisable()
        {
            _core.OnAuraFinished -= HandleAuraFinished;
            _core.StopAura();
        }

        public void OnSpawned(AuraSetup setup, Vector3 position, LayerMask enemyMask, IMemoryPool pool)
        {
            _pool = pool;
            _isDespawning = false;
            
            // Перемещаем объект в точку попадания снаряда!
            transform.position = position;
            
            _visualizer.PlayAppearAsync(setup.Radius, this.GetCancellationTokenOnDestroy()).Forget();
            _core.StartAura(setup, position, enemyMask); // Передаем position
        }

        public void OnDespawned()
        {
            _pool = null;
            _core.StopAura();
        }

        private void HandleAuraFinished()
        {
            // Защита от двойного вызова (например, если ауру отменили извне и одновременно вышло время)
            if (_isDespawning) return;
            
            DespawnRoutineAsync().Forget();
        }

        private async UniTaskVoid DespawnRoutineAsync()
        {
            _isDespawning = true;
            
            if (_visualizer != null)
            {
                // Сначала ждем, пока полусфера плавно сожмется
                await _visualizer.PlayDisappearAsync(this.GetCancellationTokenOnDestroy());
            }
            
            // Только после окончания анимации возвращаем объект в пул
            _pool?.Despawn(this);
        }

        public void Dispose()
        {
            _core?.StopAura();
        }

        public class Pool : MonoMemoryPool<AuraSetup, Vector3, LayerMask, LingeringAuraFacade> { }
    }
}