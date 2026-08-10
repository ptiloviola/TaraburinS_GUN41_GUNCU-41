using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using TpsShooter.Weapons.Core;
using TpsShooter.Weapons.Configs;
using TpsShooter.Services.Input;

namespace TpsShooter.Player.Weapons
{
    public class WeaponInventory : IDisposable
    {
        private readonly IInstantiator _instantiator;
        private readonly PlayerWeaponController _weaponController;
        private readonly WeaponTransitionService _transitionService;
        private readonly IInputService _inputService;
        
        private readonly Transform _handSocket;
        private readonly Transform _backSocket1;
        private readonly Transform _backSocket2;

        private readonly List<WeaponBase> _weapons = new List<WeaponBase>();
        private int _currentWeaponIndex = -1;
        private const float TransitionDuration = 0.35f; 
        
        private const int MaxWeapons = 2; // Жесткий лимит!
        private bool _isTransitioning = false; 

        public WeaponBase CurrentWeapon { get; private set; }

        public WeaponInventory(
            IInstantiator instantiator,
            PlayerWeaponController weaponController,
            WeaponTransitionService transitionService,
            IInputService inputService,
            Transform handSocket,
            Transform backSocket1,
            Transform backSocket2)
        {
            _instantiator = instantiator;
            _weaponController = weaponController;
            _transitionService = transitionService;
            _inputService = inputService;
            _handSocket = handSocket;
            _backSocket1 = backSocket1;
            _backSocket2 = backSocket2;

            _inputService.OnWeaponSelect += EquipWeapon;
            _inputService.OnWeaponScroll += HandleScroll;
        }

        public void AddWeapon(WeaponBase weaponPrefab, WeaponConfig config, Vector3 worldPosition)
        {
            if (_weapons.Count >= MaxWeapons)
            {
                Debug.LogWarning("[Inventory] Инвентарь полон! 2 пушки уже есть.");
                // В будущем здесь мы вызовем метод DropCurrentWeapon(), чтобы выбросить старую пушку на землю
                return; 
            }

            WeaponBase newWeapon = _instantiator.InstantiatePrefabForComponent<WeaponBase>(weaponPrefab.gameObject);
            newWeapon.Initialize(config);
            
            newWeapon.transform.position = worldPosition; 
            newWeapon.transform.rotation = Quaternion.identity;
            
            _weapons.Add(newWeapon);
            int newWeaponIndex = _weapons.Count - 1;

            if (_weapons.Count == 1) 
            {
                EquipWeapon(0);
            }
            else 
            {
                // Жестко отправляем на спину в соответствующий индекс-слот
                PutWeaponOnBack(newWeapon, newWeaponIndex); 
            }
        }

        public void EquipWeapon(int index)
        {
            if (index < 0 || index >= _weapons.Count || index == _currentWeaponIndex || _isTransitioning) return;

            _isTransitioning = true;

            if (CurrentWeapon != null)
            {
                PutWeaponOnBack(CurrentWeapon, _currentWeaponIndex);
            }

            _currentWeaponIndex = index;
            CurrentWeapon = _weapons[_currentWeaponIndex];

            _transitionService.MoveWeaponToSocket(CurrentWeapon.transform, _handSocket, TransitionDuration, () =>
            {
                _weaponController.OnWeaponEquipped(CurrentWeapon);
                _isTransitioning = false; 
            });
        }

        private void PutWeaponOnBack(WeaponBase weapon, int slotNumber)
        {
            // Жесткая привязка: 0 = Сокет 1 (например, для винтовки), 1 = Сокет 2 (например, на поясе для пистолета)
            Transform targetSocket = slotNumber == 0 ? _backSocket1 : _backSocket2;
            _transitionService.MoveWeaponToSocket(weapon.transform, targetSocket, TransitionDuration);
        }

        private void HandleScroll(int direction)
        {
            if (_weapons.Count <= 1 || _isTransitioning) return;

            int nextIndex = _currentWeaponIndex + direction;

            if (nextIndex >= _weapons.Count) nextIndex = 0;
            else if (nextIndex < 0) nextIndex = _weapons.Count - 1;

            EquipWeapon(nextIndex);
        }

        public void Dispose()
        {
            _inputService.OnWeaponSelect -= EquipWeapon;
            _inputService.OnWeaponScroll -= HandleScroll;
        }
    }
}