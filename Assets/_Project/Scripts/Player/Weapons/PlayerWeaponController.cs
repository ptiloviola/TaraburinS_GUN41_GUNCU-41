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
        
        private static readonly int IsArmedHash = Animator.StringToHash("IsArmed");

        private IWeapon[] _weapons = new IWeapon[3];
        private int _currentWeaponIndex = -1; 
        
        // Добавили публичное свойство, чтобы другие стейты (например, AimState) могли знать, есть ли в руках пушка
        public bool IsArmed { get; private set; } = false; 

        public PlayerWeaponController(Transform handSocket, Transform backSocket1, Transform backSocket2, Animator animator)
        {
            _handSocket = handSocket;
            _backSocket1 = backSocket1;
            _backSocket2 = backSocket2;
            _animator = animator;
            
            _animator.SetBool(IsArmedHash, IsArmed);
        }

        public void ToggleWeapon()
        {
            IsArmed = !IsArmed;
            _animator.SetBool(IsArmedHash, IsArmed);
            
            // TODO: Физическое перемещение префаба оружия между сокетами
        }
    }
}