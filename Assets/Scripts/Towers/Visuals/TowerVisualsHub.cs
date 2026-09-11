using UnityEngine;
using System;
using Gameplay.Towers.Behaviors;
using Gameplay.Towers.Data.Visuals;
using Gameplay.Towers.Visuals.Animators;

namespace Gameplay.Towers.Visuals
{
    public class TowerVisualsHub : MonoBehaviour, ITowerVisuals
    {
        [Header("Конфиг")]
        [SerializeField] private TowerVisualSetup _setup;

        [Header("Ссылки на меши (Логика)")]
        [SerializeField] private Transform _logicalRotator; 

        [Header("Ссылки на меши (База и Поворот)")]
        [SerializeField] private Transform _baseTransform;
        [SerializeField] private Transform _turretTransform;
        [SerializeField] private Transform _elevationPivot;
        
        [Header("Ссылки на меши (Стволы)")]
        [SerializeField] private Transform[] _barrelTransforms;
        [SerializeField] private ParticleSystem[] _muzzleFlashes;

        // НОВОЕ: Хаб сам управляет визуализатором радиуса
        [Header("Визуализация радиуса")]
        [SerializeField] private TowerRadiusVisualizer _radiusVisualizer;

        private WeaponAdapter _weaponAdapter;
        
        private TowerBuildAnimator _buildAnimator;
        private MultiBarrelAnimator _barrelAnimator;
        private VisualRotator _visualRotator;
        
        public event Action OnAttackImpact;

        public void Initialize(WeaponAdapter weaponAdapter)
        {
            _weaponAdapter = weaponAdapter;

            if (_logicalRotator == null && _weaponAdapter != null)
                _logicalRotator = _weaponAdapter.LogicalRotator;

            if (_setup != null)
            {
                if (_setup.Build != null && _baseTransform != null)
                    _buildAnimator = new TowerBuildAnimator(_baseTransform, _turretTransform, _setup.Build);

                if (_setup.Recoil != null && _setup.Recoil.Enabled && _barrelTransforms != null && _barrelTransforms.Length > 0)
                    _barrelAnimator = new MultiBarrelAnimator(_barrelTransforms, _muzzleFlashes, _setup.Recoil);

                if (_setup.Rotation != null && _setup.Rotation.Enabled && _turretTransform != null)
                {
                    _visualRotator = new VisualRotator(
                        _logicalRotator, 
                        _turretTransform, 
                        _weaponAdapter?.ElevationPivot, 
                        _elevationPivot,                
                        _setup.Rotation
                    );
                }
            }

            if (_weaponAdapter != null)
            {
                _weaponAdapter.OnBuildStarted += HandleBuildStarted;
                _weaponAdapter.OnShotFired += HandleShotFired;
            }
        }

        private void OnDestroy()
        {
            if (_weaponAdapter != null)
            {
                _weaponAdapter.OnBuildStarted -= HandleBuildStarted;
                _weaponAdapter.OnShotFired -= HandleShotFired;
            }
        }

        private void LateUpdate()
        {
            _visualRotator?.Tick(Time.deltaTime);
        }

        private void HandleBuildStarted() => _buildAnimator?.PlayBuildAnimation();
        private void HandleShotFired(Vector3 pos) => _barrelAnimator?.PlayRecoil();
        
        public void TriggerAttackImpact()
        {
            OnAttackImpact?.Invoke();
        }

        // НОВОЕ: Имплементация методов ITowerVisuals
        public void ShowRadius(float currentRadius, float upgradedRadius, float minRadius = 0f)
        {
            if (_radiusVisualizer != null)
            {
                _radiusVisualizer.ShowPreview(currentRadius, upgradedRadius, minRadius);
            }
        }

        public void HideRadius()
        {
            if (_radiusVisualizer != null)
            {
                _radiusVisualizer.HidePreview();
            }
        }
    }
}