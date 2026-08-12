using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using TpsShooter.Weapons.Core;
using TpsShooter.Services.Input;
using TpsShooter.Interactables;
using TpsShooter.Environment;

namespace TpsShooter.Player.Weapons
{
    public class WeaponInventory : IDisposable
    {
        private readonly PlayerWeaponController _weaponController;
        private readonly WeaponTransitionService _transitionService;
        private readonly IInputService _inputService;
        private readonly Transform _playerTransform; 
        
        private readonly Transform _handSocket;
        private readonly Transform _backSocket1;
        private readonly Transform _backSocket2;

        private readonly List<WeaponBase> _weapons = new List<WeaponBase>();
        private int _currentWeaponIndex = -1;
        private const float TransitionDuration = 0.35f; 
        private const int MaxWeapons = 2; 
        private bool _isTransitioning = false;
        private readonly LootFactory _lootFactory;

        public bool IsFull => _weapons.Count >= MaxWeapons;
        public WeaponBase CurrentWeapon { get; private set; }

        public WeaponInventory(
            PlayerWeaponController weaponController,
            WeaponTransitionService transitionService,
            IInputService inputService,
            Transform playerTransform, 
            Transform handSocket,
            Transform backSocket1,
            Transform backSocket2,
            LootFactory lootFactory)
        {
            _weaponController = weaponController;
            _transitionService = transitionService;
            _inputService = inputService;
            _playerTransform = playerTransform;
            _handSocket = handSocket;
            _backSocket1 = backSocket1;
            _backSocket2 = backSocket2;
            _lootFactory = lootFactory;

            _inputService.OnWeaponSelect += EquipWeapon;
            _inputService.OnWeaponScroll += HandleScroll;
            _inputService.OnDropWeapon += DropCurrentWeapon;
        }

        public bool HasWeapon(string weaponName)
        {
            foreach (var weapon in _weapons)
            {
                if (weapon.Config.WeaponName == weaponName) return true;
            }
            return false;
        }

        // Теперь мы принимаем ЖИВУЮ пушку
        public void AddWeapon(WeaponBase weaponInstance)
        {
            if (IsFull) return;

            // 1. Убиваем анимацию левитации, чтобы пушка не дергалась в руках
            if (weaponInstance.TryGetComponent(out PickupAnimator animator))
            {
                GameObject.Destroy(animator);
            }
            
            // 2. Включаем мозги пушке
            weaponInstance.enabled = true;
            
            _weapons.Add(weaponInstance);
            int newWeaponIndex = _weapons.Count - 1;

            if (_weapons.Count == 1) EquipWeapon(0);
            else PutWeaponOnBack(weaponInstance, newWeaponIndex); 
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

            Debug.Log($"<color=green>[Inventory]</color> Экипируем оружие: {CurrentWeapon.Config.WeaponName}");
            _transitionService.MoveWeaponToSocket(CurrentWeapon.transform, _handSocket, TransitionDuration, () =>
            {
                _weaponController.OnWeaponEquipped(CurrentWeapon);
                _isTransitioning = false; 
            });
        }

        private void DropCurrentWeapon()
        {
            if (_weapons.Count == 0 || CurrentWeapon == null || _isTransitioning) return;

            WeaponBase weaponToDrop = CurrentWeapon;
            
            Debug.Log($"<color=yellow>[Inventory]</color> Выбрасываем оружие: {weaponToDrop.Config.WeaponName}");

            // 1. Убираем из инвентаря
            _weapons.RemoveAt(_currentWeaponIndex);
            CurrentWeapon = null;
            _currentWeaponIndex = -1;

            // 2. Рассчитываем позицию сброса
            Vector3 dropPos = _playerTransform.position + _playerTransform.forward * 2.0f + _playerTransform.right * 1.5f + Vector3.up * 0.5f;
            
            // 3. ВЫЗЫВАЕМ ФАБРИКУ (вся грязная работа теперь там)
            _lootFactory.DropLiveWeapon(weaponToDrop, dropPos, Quaternion.identity);

            // 4. Достаем оставшееся оружие (или остаемся с пустыми руками)
            if (_weapons.Count > 0)
            {
                Debug.Log($"<color=cyan>[Inventory]</color> Достаем оставшееся оружие: {_weapons[0].Config.WeaponName}");
                EquipWeapon(0); 
            }
            else
            {
                Debug.Log($"<color=cyan>[Inventory]</color> Игрок остался с пустыми руками.");
                _weaponController.OnWeaponEquipped(null);
            }
        }

        private void PutWeaponOnBack(WeaponBase weapon, int slotNumber)
        {
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
            _inputService.OnDropWeapon -= DropCurrentWeapon;
        }
    }
}