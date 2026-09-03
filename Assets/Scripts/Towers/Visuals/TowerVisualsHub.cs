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

        private WeaponAdapter _weaponAdapter;
        
        // Чистые классы (невидимы в инспекторе, живут только в памяти)
        private TowerBuildAnimator _buildAnimator;
        private MultiBarrelAnimator _barrelAnimator;
        private VisualRotator _visualRotator;
        
        public event Action OnAttackImpact;

        public void Initialize(WeaponAdapter weaponAdapter)
        {
            _weaponAdapter = weaponAdapter;

            // Если логический ротатор не назначен, ищем его в адаптере
            if (_logicalRotator == null && _weaponAdapter != null)
                _logicalRotator = _weaponAdapter.LogicalRotator;

            // 1. Инициализируем чистые классы, если конфиги включены
            if (_setup != null)
            {
                if (_setup.Build != null && _baseTransform != null)
                    _buildAnimator = new TowerBuildAnimator(_baseTransform, _turretTransform, _setup.Build);

                if (_setup.Recoil != null && _setup.Recoil.Enabled && _barrelTransforms != null && _barrelTransforms.Length > 0)
                    _barrelAnimator = new MultiBarrelAnimator(_barrelTransforms, _muzzleFlashes, _setup.Recoil);

                if (_setup.Rotation != null && _setup.Rotation.Enabled && _turretTransform != null)
                    _visualRotator = new VisualRotator(_logicalRotator, _turretTransform, _elevationPivot, _setup.Rotation);
            }

            // 2. Подписываемся на события логики
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
            // Хаб берет на себя ответственность дергать чистый ротатор каждый кадр
            _visualRotator?.Tick(Time.deltaTime);
        }

        private void HandleBuildStarted() => _buildAnimator?.PlayBuildAnimation();
        private void HandleShotFired(Vector3 pos) => _barrelAnimator?.PlayRecoil();
        
        // Метод для интерфейса ITowerVisuals (для башен ближнего боя)
        public void TriggerAttackImpact()
        {
            OnAttackImpact?.Invoke();
        }
    }
}