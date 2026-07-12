using UnityEngine;
using Services;
using UnityEngine.Audio;
using Infrastructure.Interfaces;
using Player.Weapon.Visuals;
using Player.UI;
using Player.Weapon.Physics;
using Player.Weapon.Effects;
using Player.Weapon;
using Player.Input;
using Player.Config;
using Player.Weapon.Config;
using Player.Audio;

namespace Player
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInputHandler))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _cameraTransform; 

        [Header("Configs")]
        [SerializeField] private MovementConfig _movementConfig;
        [SerializeField] private RadarConfig _radarConfig;
        [SerializeField] private RangedWeaponConfig _weaponConfig;

        [Header("Weapon References")]
        [SerializeField] private Transform _weaponRoot;
        [SerializeField] private Transform _visualsRoot;
        [SerializeField] private Transform _cylinder;
        [SerializeField] private Transform _hammer;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private Transform _trigger;

        [Header("Audio")]
        [SerializeField] private AudioMixerGroup _sfxGroup;
        [SerializeField] private AudioSource _playerSoundSource;

        [Header("UI References")]
        [SerializeField] private RectTransform _crosshairUi;

        private PlayerMovement _movement;
        private PlayerLook _look;
        private PlayerVision _vision;
        private WeaponController _weapon;
        
        private CharacterController _characterController;
        private AudioSource _audioSource;
        private AudioService _audioService;
        private IInputProvider _input;
        private PlayerFootsteps _footsteps;

        private void Awake()
        {
            _input = GetComponent<IInputProvider>();
            _characterController = GetComponent<CharacterController>();
            _audioSource = GetComponent<AudioSource>();


            _audioService = new AudioService(_sfxGroup);
            

            IWeaponView weaponView = new RevolverVisualView(_weaponRoot, _visualsRoot, _cylinder, _hammer, _trigger, _weaponConfig);
            ICrosshairView crosshairView = new CrosshairUiView(_crosshairUi, _weaponConfig);
            IShooter shooter = new RaycastShooter(_cameraTransform);
            IImpactHandler impactHandler = new PaintballImpactHandler(_weaponConfig, _audioService);


            _weapon = new WeaponController(weaponView, crosshairView, shooter, impactHandler, 
                _audioService, GetComponent<AudioSource>(), _firePoint, _weaponConfig, _input);
            

            _movement = new PlayerMovement(_characterController, transform, _movementConfig);
            _look = new PlayerLook(transform, _cameraTransform, _movementConfig); // Предполагается, что настройки чувствительности мыши теперь в MovementConfig
            _vision = new PlayerVision(_cameraTransform, _radarConfig);
            _footsteps = new PlayerFootsteps(_playerSoundSource, _characterController, _input);
        }

        private void Update()
        {
            _movement.Tick(_input.MoveInput, Time.deltaTime);
            _look.Tick(_input.LookInput, Time.deltaTime);
            _vision.Tick();
            _weapon.Tick();
            _footsteps.Tick();
        }

        private void OnDestroy()
        {
            _weapon?.Dispose(); 
        }
    }
}