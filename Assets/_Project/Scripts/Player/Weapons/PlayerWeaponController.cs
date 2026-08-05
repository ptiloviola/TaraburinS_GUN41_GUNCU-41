using UnityEngine;
using TpsShooter.Weapons;

namespace TpsShooter.Player.Weapons
{
    public class PlayerWeaponController
    {
        private readonly Transform _handSocket;
        private readonly Transform _backSocket1;
        private readonly Transform _backSocket2;
        private readonly Animator _animator;
        private readonly Transform _leftHandIkTarget;
        
        private static readonly int IsArmedHash = Animator.StringToHash("IsArmed");

        private IWeapon[] _weapons = new IWeapon[3];
        private int _currentWeaponIndex = -1; 

        public WeaponBase CurrentWeapon { get; private set; }
        
        // Добавили публичное свойство, чтобы другие стейты (например, AimState) могли знать, есть ли в руках пушка
        public bool IsArmed { get; private set; } = false; 

        public PlayerWeaponController(Transform handSocket, Transform backSocket1, 
            Transform backSocket2, Animator animator, Transform leftHandIkTarget)
        {
            _handSocket = handSocket;
            _backSocket1 = backSocket1;
            _backSocket2 = backSocket2;
            _animator = animator;
            
            _animator.SetBool(IsArmedHash, IsArmed);
            _leftHandIkTarget = leftHandIkTarget;
        }

        public void ToggleWeapon()
        {
            IsArmed = !IsArmed;
            _animator.SetBool(IsArmedHash, IsArmed);
            
            // TODO: Физическое перемещение префаба оружия между сокетами
        }

        // Временный метод для теста: спавним префаб прямо в руку
        public void TestEquipWeapon(WeaponBase weaponPrefab)
        {
            if (CurrentWeapon != null)
                Object.Destroy(CurrentWeapon.gameObject); // Для теста удаляем старое

            // Создаем пушку на сцене
            CurrentWeapon = Object.Instantiate(weaponPrefab);
            
            // Сажаем в правый сокет (поворот и позицию подгонишь в инспекторе оружия)
            CurrentWeapon.SetParent(_handSocket, Vector3.zero, Vector3.zero);
            CurrentWeapon.Initialize();

            IsArmed = true;
            _animator.SetBool(IsArmedHash, IsArmed);

            // МАГИЯ IK: Привязываем таргет левой руки к точке на самом оружии!
            if (CurrentWeapon.LeftHandGripPoint != null && _leftHandIkTarget != null)
            {
                _leftHandIkTarget.SetParent(CurrentWeapon.LeftHandGripPoint);
                _leftHandIkTarget.localPosition = Vector3.zero;
                // _leftHandIkTarget.localRotation = Quaternion.identity;
            }
        }
    }
}