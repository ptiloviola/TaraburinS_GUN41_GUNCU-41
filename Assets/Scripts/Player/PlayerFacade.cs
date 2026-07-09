using UnityEngine;
using UnityEngine.InputSystem;
using Services;
using UnityEngine.Audio;
using Infrastructure.Interfaces;
using Player.Weapon.Visuals;
using Player.UI;
using Player.Weapon.Physics;
using Player.Weapon.Effects;
using Player.Weapon;


namespace Player
{

    [RequireComponent(typeof(CharacterController))]
    public class PlayerFacade : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerConfig config;
        [SerializeField] private Transform cameraTransform; 

        [Header("Input Actions")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction; // Добавили экшен для мыши
        [SerializeField] private InputActionReference aimAction;
        [SerializeField] private InputActionReference fireAction;
        [SerializeField] private InputActionReference reloadAction;

        [Header("Weapon References")]
        [SerializeField] private Transform weaponRoot;      // Пустышка-родитель оружия
        [SerializeField] private Transform visualsRoot;     // Пустышка внутри Root (или сам меш body_low)
        [SerializeField] private Transform cylinder;        // Барабан
        [SerializeField] private Transform hammer;          // Курок
        [SerializeField] private Transform firePoint;       // Пустышка на конце дула
        [SerializeField] private Transform trigger;

        [Header("Audio")]
        [SerializeField] private AudioMixerGroup sfxGroup;

        [Header("UI References")]
        [SerializeField] private RectTransform crosshairUi;


        private PlayerMovement _movement;
        private PlayerLook _look;
        private PlayerVision _vision;
        private WeaponController _weapon;
        
        private CharacterController _characterController;
        private AudioSource _audioSource;
        private AudioService _audioService;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _audioSource = GetComponent<AudioSource>();

            // 1. Инициализируем инфраструктурные сервисы
            _audioService = new AudioService(sfxGroup);
            
            // 2. Создаем узкоспециализированные компоненты оружия
            IWeaponView weaponView = new RevolverVisualView(weaponRoot, visualsRoot, cylinder, hammer, trigger, config);
            ICrosshairView crosshairView = new CrosshairUiView(crosshairUi, config);
            IShooter shooter = new RaycastShooter(cameraTransform);
            IImpactHandler impactHandler = new PaintballImpactHandler(config, _audioService);

            // 3. Собираем основное ядро оружия, передавая ему компоненты через интерфейсы
            _weapon = new WeaponController(weaponView, crosshairView, shooter, impactHandler, _audioService, _audioSource, firePoint, config);

            // Инициализируем системы движения и радара
            _movement = new PlayerMovement(_characterController, transform, config);
            _look = new PlayerLook(transform, cameraTransform, config);
            _vision = new PlayerVision(cameraTransform, config);
        }

        private void OnEnable()
        {
            // ОБЯЗАТЕЛЬНО включаем экшены, иначе ReadValue будет возвращать нули
            moveAction.action.Enable();
            lookAction.action.Enable();
            aimAction.action.Enable();
            fireAction.action.Enable();
            reloadAction.action.Enable();

            aimAction.action.started += ctx => _weapon.ToggleAim(true);
            aimAction.action.canceled += ctx => _weapon.ToggleAim(false);
            fireAction.action.started += ctx => _weapon.Fire();
            reloadAction.action.started += ctx => _weapon.Reload();
        }

        private void OnDisable()
        {
            // Отключаем экшены и отписываемся от событий при выключении объекта
            moveAction.action.Disable();
            lookAction.action.Disable();
            aimAction.action.Disable();
            fireAction.action.Disable();
            reloadAction.action.Disable();

            aimAction.action.started -= ctx => _weapon.ToggleAim(true);
            aimAction.action.canceled -= ctx => _weapon.ToggleAim(false);
            fireAction.action.started -= ctx => _weapon.Fire();
            reloadAction.action.started -= ctx => _weapon.Reload();
        }

        private void Update()
        {
            // Читаем ввод
            Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
            Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

            // Передаем управление в чистые классы
            _movement.Tick(moveInput, Time.deltaTime);
            _look.Tick(lookInput, Time.deltaTime);
            _vision.Tick();
            
            bool isMoving = moveInput.sqrMagnitude > 0.01f;
            _weapon.PlayBobbing(isMoving);
        }
    }
}