using System;
using UnityEngine;
using Zenject;
using Gameplay.Auras.Data;
using Gameplay.Auras.Visuals;

namespace Gameplay.Auras
{
    public class LingeringAuraFacade : MonoBehaviour, IDisposable
    {
        private AuraZoneVisualizer _visualizer; 
        public AuraCore Core { get; private set; }
        
        private IMemoryPool _pool;

        [Inject]
        public void Construct(IMemoryPool pool)
        {
            Core = new AuraCore();
            _pool = pool;
        }

        private void OnEnable()
        {
            if (Core != null) Core.OnAuraFinished += Despawn;
        }

        private void OnDisable()
        {
            if (Core != null)
            {
                Core.OnAuraFinished -= Despawn;
                Core.StopAura();
            }
        }

        public void InitializeAura(AuraSetup setup, Vector3 position, LayerMask enemyMask)
        {
            transform.position = position;

            if (_visualizer == null) _visualizer = GetComponent<AuraZoneVisualizer>();
            
            if (_visualizer != null) 
            {
                _visualizer.PlayAppear(setup.Radius);
            }
                
            Core.StartAura(setup, position, enemyMask);
        }

        private void Despawn()
        {
            if (_visualizer != null)
            {
                _visualizer.PlayDisappear(() => _pool?.Despawn(this));
            }
            else
            {
                _pool?.Despawn(this);
            }
        }

        public void Dispose()
        {
            Core?.StopAura();
        }

        public class Pool : MonoMemoryPool<AuraSetup, Vector3, LayerMask, LingeringAuraFacade> 
        {
            protected override void Reinitialize(AuraSetup setup, Vector3 position, LayerMask mask, LingeringAuraFacade item)
            {
                // Отрабатывает базовая логика Zenject (достает из пула, включает объект)
                base.Reinitialize(setup, position, mask, item);
                
                // Передаем данные напрямую без рефлексии
                item.InitializeAura(setup, position, mask);
            }
        }
    }
}