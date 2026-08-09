using System.Collections.Generic;
using UnityEngine;
using Zenject;
using TpsShooter.Weapons.Core;
using TpsShooter.Weapons.Configs;

namespace TpsShooter.Player.Weapons
{
    public class WeaponInventory
    {
        private readonly IInstantiator _instantiator;
        private readonly PlayerWeaponController _weaponController;
        private readonly WeaponTransitionService _transitionService; // Наш новый сервис
        
        private readonly Transform _handSocket;
        private readonly Transform _backSocket1;
        private readonly Transform _backSocket2;

        private readonly List<WeaponBase> _weapons = new List<WeaponBase>();
        private int _currentWeaponIndex = -1;
        private const float TransitionDuration = 0.35f; // Время анимации переброса

        public WeaponBase CurrentWeapon { get; private set; }

        public WeaponInventory(
            IInstantiator instantiator,
            PlayerWeaponController weaponController,
            WeaponTransitionService transitionService, // Добавили в конструктор
            Transform handSocket,
            Transform backSocket1,
            Transform backSocket2)
        {
            _instantiator = instantiator;
            _weaponController = weaponController;
            _transitionService = transitionService;
            _handSocket = handSocket;
            _backSocket1 = backSocket1;
            _backSocket2 = backSocket2;
        }

        // Добавили worldPosition, чтобы пушка летела оттуда, где лежала
        public void AddWeapon(WeaponBase weaponPrefab, WeaponConfig config, Vector3 worldPosition)
        {
            if (_weapons.Count >= 3)
            {
                Debug.LogWarning("Инвентарь полон!");
                return;
            }

            WeaponBase newWeapon = _instantiator.InstantiatePrefabForComponent<WeaponBase>(weaponPrefab.gameObject);
            newWeapon.Initialize(config);
            
            // Ставим пушку туда, где был пикап
            newWeapon.transform.position = worldPosition; 
            newWeapon.transform.rotation = Quaternion.identity;
            
            _weapons.Add(newWeapon);

            if (_weapons.Count == 1)
            {
                // Если это первая пушка, летим сразу в руку
                EquipWeapon(0);
            }
            else
            {
                // Если рука занята, пушка летит за спину
                PutWeaponOnBack(newWeapon, _weapons.Count);
            }
        }

        public void EquipWeapon(int index)
        {
            if (index < 0 || index >= _weapons.Count || index == _currentWeaponIndex) return;

            // Если в руках что-то есть, плавно убираем за спину
            if (CurrentWeapon != null)
            {
                PutWeaponOnBack(CurrentWeapon, _currentWeaponIndex + 1);
            }

            _currentWeaponIndex = index;
            CurrentWeapon = _weapons[_currentWeaponIndex];

            // Плавно достаем новое оружие в руку
            _transitionService.MoveWeaponToSocket(CurrentWeapon.transform, _handSocket, TransitionDuration, () =>
            {
                // Оповещаем контроллер (чтобы левая рука прилипла через IK) ТОЛЬКО когда пушка долетела!
                _weaponController.OnWeaponEquipped(CurrentWeapon);
            });
        }

        private void PutWeaponOnBack(WeaponBase weapon, int slotNumber)
        {
            // Четко привязываем индекс слота к нужному сокету
            Transform targetSocket = slotNumber == 0 ? _backSocket1 : _backSocket2;
            _transitionService.MoveWeaponToSocket(weapon.transform, targetSocket, TransitionDuration);
            
        }

        public void ToggleNextWeapon()
        {
            // Если пушек нет или она всего одна — переключать нечего
            if (_weapons.Count <= 1) return;

            // Считаем индекс следующего оружия по кругу
            int nextIndex = (_currentWeaponIndex + 1) % _weapons.Count;
            
            // Вызываем уже готовый метод экипировки
            EquipWeapon(nextIndex);
        }
    }
}