using UnityEngine;
using UnityEngine.Animations.Rigging;
using TpsShooter.Weapons.Core;
using TpsShooter.Services.Input; // Обязательно подключаем инпут

namespace TpsShooter.Player.Weapons
{
    public class PlayerWeaponController
    {
        private readonly Transform _handSocket;
        private readonly Transform _backSocket1;
        private readonly Transform _backSocket2;
        private readonly Animator _animator;
        private readonly Transform _leftHandIkTarget;
        private readonly Rig _weaponRig;
        private readonly TwoBoneIKConstraint _leftHandIK;
        
        // Новые зависимости для стрельбы
        private readonly IInputService _inputService;
        private readonly Transform _cameraTransform;
        private readonly Transform _aimTarget;
        
        private static readonly int IsArmedHash = Animator.StringToHash("IsArmed");
        private static readonly int MeleePunchStateHash = Animator.StringToHash("MeleePunch");
        private static readonly int RollStateHash = Animator.StringToHash("Roll");
        private const int UpperBodyLayerIndex = 1;

        public WeaponBase CurrentWeapon { get; private set; }
        public bool IsArmed { get; private set; } = false; 
        public bool IsAiming { get; private set; } = false;

        private bool _wasFiring = false;

        public PlayerWeaponController(
            Transform handSocket, 
            Transform backSocket1, 
            Transform backSocket2, 
            Animator animator, 
            Transform leftHandIkTarget, 
            Rig weaponRig, 
            TwoBoneIKConstraint leftHandIK,
            IInputService inputService,       // + Инпут
            Transform cameraTransform,        // + Камера
            Transform aimTarget)              // + Таргет
        {
            _handSocket = handSocket;
            _backSocket1 = backSocket1;
            _backSocket2 = backSocket2;
            _animator = animator;
            _leftHandIkTarget = leftHandIkTarget;
            _weaponRig = weaponRig;
            _leftHandIK = leftHandIK;
            
            _inputService = inputService;
            _cameraTransform = cameraTransform;
            _aimTarget = aimTarget;
            
            _animator.SetBool(IsArmedHash, IsArmed);
            
            if (_weaponRig != null) _weaponRig.weight = 0f;
            if (_leftHandIK != null) _leftHandIK.weight = 0f;
        }

        public void OnWeaponEquipped(WeaponBase newWeapon)
        {
            CurrentWeapon = newWeapon;
            IsArmed = true;
            _animator.SetBool(IsArmedHash, IsArmed);
        }

        public void SetAiming(bool isAiming)
        {
            IsAiming = isAiming;
        }

        public void Tick(float deltaTime)
        {
            // --- 1. ГЛОБАЛЬНЫЙ ПРИЦЕЛ И СТРЕЛЬБА ---
            if (_cameraTransform != null && _aimTarget != null)
            {
                // =========================================================
                // А. ЖЕСТКАЯ ПРИВЯЗКА ВИЗУАЛЬНОГО ТАРГЕТА (БЕЗ ФИЗИКИ)
                // =========================================================
                // Мы ЖЕСТКО говорим кубику: виси в 50 метрах перед камерой. 
                // Никаких лучей. Никаких проверок на столкновения.
                // Благодаря этому спина НИКОГДА не будет дергаться.
                _aimTarget.position = _cameraTransform.position + _cameraTransform.forward * 50f;

                // =========================================================
                // Б. ФИЗИЧЕСКИЙ ЛУЧ (ТОЛЬКО ДЛЯ ПУЛЬ)
                // =========================================================
                // По умолчанию пули полетят туда же, куда смотрит спина
                Vector3 shootTargetPosition = _aimTarget.position;

                if (IsArmed && CurrentWeapon != null && CurrentWeapon.Config != null)
                {
                    int safeMask = CurrentWeapon.Config.HitMask & ~LayerMask.GetMask("Player", "Ignore Raycast");

                    // Кидаем физический луч сквозь уровень
                    if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit camHit, 100f, safeMask))
                    {
                        // Если луч во что-то врезался (стена, враг, пол), 
                        // мы меняем цель ДЛЯ СТРЕЛЬБЫ, но НЕ ТРОГАЕМ AimTarget!
                        shootTargetPosition = camHit.point; 
                    }
                }

                // =========================================================
                // В. ОБРАБОТКА СТРЕЛЬБЫ
                // =========================================================
                bool isFiringNow = _inputService != null && _inputService.IsFiring;
                bool isTriggerPulled = isFiringNow && !_wasFiring; 
                _wasFiring = isFiringNow; 

                if (IsArmed && CurrentWeapon != null && CurrentWeapon.Config != null)
                {
                    bool canFire = CurrentWeapon.Config.IsAutomatic ? isFiringNow : isTriggerPulled;
                    if (canFire)
                    {
                        // Передаем пушке физическую точку попадания
                        CurrentWeapon.TryFire(shootTargetPosition);
                    }
                }
            }

            // --- 2. УМНОЕ ВЫЧИСЛЕНИЕ ВЕСОВ (ТЕПЕРЬ ПРАВИЛЬНОЕ) ---
            bool isMeleeing = _animator.GetCurrentAnimatorStateInfo(UpperBodyLayerIndex).shortNameHash == MeleePunchStateHash;
            bool isRolling = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash == RollStateHash;

            // 1. Вес слоя рук: 1, если мы вооружены (чтобы держать пушку от бедра), НО строго 0 во время переката
            float targetLayerWeight = (!isRolling) ? 1f : 0f;
            
            // 2. Вес Риггинга (спина): 1, ТОЛЬКО когда мы целимся (ПКМ) и не бьем/не кувыркаемся!
            float targetRigWeight = (IsArmed && IsAiming && !isMeleeing && !isRolling) ? 1f : 0f;

            // 3. Вес левой руки: 1, всегда когда с пушкой, кроме рукопашки и переката
            float leftHandTargetWeight = (IsArmed && !isMeleeing && !isRolling) ? 1f : 0f;
            
            float lerpSpeed = deltaTime * 15f;

            // --- 3. ПРИМЕНЯЕМ ВЕСА ---
            // Слой Аниматора
            float currentLayerWeight = _animator.GetLayerWeight(UpperBodyLayerIndex);
            _animator.SetLayerWeight(UpperBodyLayerIndex, Mathf.Lerp(currentLayerWeight, targetLayerWeight, lerpSpeed));

            // Поворот спины (теперь сработает только при ПКМ!)
            if (_weaponRig != null) 
            {
                _weaponRig.weight = Mathf.Lerp(_weaponRig.weight, targetRigWeight, lerpSpeed);
            }

            // Смещение оружия к лицу (ADS)
            if (IsArmed && CurrentWeapon != null)
            {
                Transform weaponTransform = CurrentWeapon.transform;
                if (IsAiming && !isMeleeing)
                {
                    weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, CurrentWeapon.AimPositionOffset, lerpSpeed);
                    weaponTransform.localRotation = Quaternion.Slerp(weaponTransform.localRotation, Quaternion.Euler(CurrentWeapon.AimRotationOffset), lerpSpeed);
                }
                else
                {
                    weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, Vector3.zero, lerpSpeed);
                    weaponTransform.localRotation = Quaternion.Slerp(weaponTransform.localRotation, Quaternion.identity, lerpSpeed);
                }
            }

            // Левая рука (теперь не зависит от того, гнется спина или нет)
            if (!IsArmed || CurrentWeapon == null || CurrentWeapon.LeftHandGripPoint == null)
            {
                if (_leftHandIK != null) _leftHandIK.weight = Mathf.Lerp(_leftHandIK.weight, 0f, lerpSpeed);
                return;
            }

            if (_leftHandIkTarget != null)
            {
                _leftHandIkTarget.position = CurrentWeapon.LeftHandGripPoint.position;
                _leftHandIkTarget.rotation = CurrentWeapon.LeftHandGripPoint.rotation;
                
                if (_leftHandIK != null) 
                {
                    _leftHandIK.weight = Mathf.Lerp(_leftHandIK.weight, leftHandTargetWeight, lerpSpeed * 2f);
                }
            }
        }


    }
}