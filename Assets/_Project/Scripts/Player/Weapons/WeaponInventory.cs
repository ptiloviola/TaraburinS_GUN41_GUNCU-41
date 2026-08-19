using System;
using System.Collections.Generic;
using UnityEngine;
using TpsShooter.Weapons.Core;
using TpsShooter.Services.Input;
using TpsShooter.Interactables;
using TpsShooter.Environment;
using Cysharp.Threading.Tasks;
using System.Threading;

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
        
        private readonly LootFactory _lootFactory;
        
        private CancellationTokenSource _transitionCts;

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

        public void AddWeapon(WeaponBase weaponInstance)
        {
            if (IsFull) return;

            if (weaponInstance.TryGetComponent(out PickupAnimator animator))
            {
                GameObject.Destroy(animator);
            }
            
            weaponInstance.enabled = true;
            _weapons.Add(weaponInstance);
            int newWeaponIndex = _weapons.Count - 1;

            if (_weapons.Count == 1) EquipWeapon(0);
            else PutWeaponOnBackAsync(weaponInstance, newWeaponIndex, CancellationToken.None).Forget(); 
        }

        public void EquipWeapon(int index) => EquipWeaponAsync(index).Forget();

        private async UniTaskVoid EquipWeaponAsync(int index)
        {
            if (index < 0 || index >= _weapons.Count || index == _currentWeaponIndex) return;

            _transitionCts?.Cancel();
            _transitionCts = new CancellationTokenSource();
            var token = _transitionCts.Token;

            try
            {
                if (CurrentWeapon != null)
                {
                    CurrentWeapon.CancelReload(); 
                    
                    await PutWeaponOnBackAsync(CurrentWeapon, _currentWeaponIndex, token);
                }

                _currentWeaponIndex = index;
                CurrentWeapon = _weapons[_currentWeaponIndex];

                DevLogger.Log($"<color=green>[Inventory]</color> Экипируем оружие: {CurrentWeapon.Config.WeaponName}");
                
                await _transitionService.MoveWeaponToSocketAsync(CurrentWeapon.transform, _handSocket, TransitionDuration, token);

                _weaponController.OnWeaponEquipped(CurrentWeapon);
            }
            catch (OperationCanceledException)
            {
                DevLogger.Log("<color=yellow>[Inventory]</color> Смена оружия прервана новым вводом!");
            }
        }

        private async UniTask PutWeaponOnBackAsync(WeaponBase weapon, int slotNumber, CancellationToken token)
        {
            Transform targetSocket = slotNumber == 0 ? _backSocket1 : _backSocket2;
            await _transitionService.MoveWeaponToSocketAsync(weapon.transform, targetSocket, TransitionDuration, token);
        }

        private void DropCurrentWeapon()
        {
            if (_weapons.Count == 0 || CurrentWeapon == null) return;
            
            _transitionCts?.Cancel();

            WeaponBase weaponToDrop = CurrentWeapon;
            weaponToDrop.CancelReload();
            
            DevLogger.Log($"<color=yellow>[Inventory]</color> Выбрасываем оружие: {weaponToDrop.Config.WeaponName}");

            _weapons.RemoveAt(_currentWeaponIndex);
            CurrentWeapon = null;
            _currentWeaponIndex = -1;

            Vector3 dropPos = _playerTransform.position + _playerTransform.forward * 2.0f + _playerTransform.right * 1.5f + Vector3.up * 0.5f;
            _lootFactory.DropLiveWeapon(weaponToDrop, dropPos, Quaternion.identity);

            if (_weapons.Count > 0)
            {
                EquipWeapon(0); 
            }
            else
            {
                _weaponController.OnWeaponEquipped(null);
            }
        }

        private void HandleScroll(int direction)
        {
            if (_weapons.Count <= 1) return;

            int nextIndex = _currentWeaponIndex + direction;
            if (nextIndex >= _weapons.Count) nextIndex = 0;
            else if (nextIndex < 0) nextIndex = _weapons.Count - 1;

            EquipWeapon(nextIndex);
        }

        public void CancelAllOperations()
        {
            _transitionCts?.Cancel();
            CurrentWeapon?.CancelReload();
        }

        public void Dispose()
        {
            CancelAllOperations();
            _transitionCts?.Dispose();
            
            _inputService.OnWeaponSelect -= EquipWeapon;
            _inputService.OnWeaponScroll -= HandleScroll;
            _inputService.OnDropWeapon -= DropCurrentWeapon;
        }
    }
}