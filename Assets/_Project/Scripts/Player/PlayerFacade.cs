using UnityEngine;
using Zenject;
using TpsShooter.Services.Input;
using TpsShooter.Player.States;
using TpsShooter.Player.Core;
using TpsShooter.Player.Configs;
using TpsShooter.Player.Camera;
using TpsShooter.Player.Weapons;
using TpsShooter.Player.Combat;
using UnityEngine.Animations.Rigging;
using TpsShooter.Player.Inventory;
using TpsShooter.Environment;
using TpsShooter.Combat;
using TpsShooter.Audio;


namespace TpsShooter.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerFacade : MonoBehaviour, IDamageable
    {
        private PlayerStateMachine _stateMachine;
        private PlayerContext _context;
        private IInputService _inputService;
        
        [Header("Camera Settings")]
        [SerializeField] private Cinemachine.CinemachineFreeLook _normalCamera;
        [SerializeField] private Transform _cameraTarget;
        
        [Header("Weapon & Rigging Settings")]
        [SerializeField] private Transform _aimTarget;
        [SerializeField] private Rig _weaponRig;
        [SerializeField] private TwoBoneIKConstraint _leftHandIK;
        
        [SerializeField] private Transform _weaponHandSocket;
        [SerializeField] private Transform _weaponBackSocket1;
        [SerializeField] private Transform _weaponBackSocket2;
        [SerializeField] private Transform _leftHandIkTarget;

        private PlayerCameraController _cameraController;
        private PlayerWeaponController _weaponController;
        private WeaponInventory _weaponInventory;
        private PlayerInteractionSensor _interactionSensor;
        
        // Новые компоненты для Melee
        private PlayerMeleeController _meleeController;
        private PlayerAnimationEvents _animEvents;
        private FootstepAudioSystem _footstepAudio;
        private IAudioService _audioService;

        public HealthEngine Health { get; private set; }

        public float CurrentWeaponSpread 
        {
            get 
            {
                if (_weaponController != null && _weaponController.CurrentWeapon != null)
                    return _weaponController.CurrentWeapon.CurrentSpread;
                return 0f;
            }
        }
        public WeaponInventory WeaponInventory => _weaponInventory;
        public PlayerInventoryModel InventoryModel { get; private set; } 

        [Inject]
        public void Construct(
            IInputService inputService, 
            PlayerConfig config, 
            PlayerInventoryConfig inventoryConfig, // <--- НОВЫЙ КОНФИГ
            IInstantiator instantiator,
            PlayerInventoryModel inventoryModel,
            LootFactory lootFactory,
            IAudioService audioService,
            FootstepConfig footstepConfig)
        {
            _audioService = audioService;
            _inputService = inputService;
            Transform camTransform = UnityEngine.Camera.main != null ? UnityEngine.Camera.main.transform : null;
            var groundSensor = new GroundSensor(transform, config);
            Animator animator = GetComponentInChildren<Animator>();
            
            _cameraController = new PlayerCameraController(config, _cameraTarget, _normalCamera);
            
            _weaponController = new PlayerWeaponController(
                _weaponHandSocket, _weaponBackSocket1, _weaponBackSocket2, 
                animator, _leftHandIkTarget, _weaponRig, _leftHandIK,
                inputService, camTransform, _aimTarget);

            WeaponTransitionService transitionService = new WeaponTransitionService(this);

            _weaponInventory = new WeaponInventory(
                _weaponController, transitionService, inputService, transform, 
                _weaponHandSocket, _weaponBackSocket1, _weaponBackSocket2,
                lootFactory);

            _interactionSensor = new PlayerInteractionSensor(transform);

            InventoryModel = inventoryModel;

            Health = new HealthEngine(config.MaxHealth);
            Health.OnDeath += HandleDeath;

            


            // --- ИНИЦИАЛИЗАЦИЯ БЛИЖНЕГО БОЯ ---
            _meleeController = new PlayerMeleeController(animator, transform);
            
            // Навешиваем слушатель событий анимации на ту же пустышку, где висит Animator
            _animEvents = animator.gameObject.GetComponent<PlayerAnimationEvents>();
            if (_animEvents == null) 
                _animEvents = animator.gameObject.AddComponent<PlayerAnimationEvents>();
                
            _animEvents.OnMeleeStrike += _meleeController.PerformStrike;

            // -----------------------------------

            _context = new PlayerContext(
                GetComponent<CharacterController>(), transform, camTransform, config,
                inputService, groundSensor, animator, this, _cameraController,
                _aimTarget, _weaponRig, _weaponController, _weaponInventory);

            _stateMachine = new PlayerStateMachine();
            _stateMachine.AddState(new PlayerIdleState(_context, _stateMachine));
            _stateMachine.AddState(new PlayerMoveState(_context, _stateMachine));
            _stateMachine.AddState(new PlayerAimState(_context, _stateMachine));
            _stateMachine.AddState(new PlayerAirborneState(_context, _stateMachine));
            _stateMachine.AddState(new PlayerCrouchState(_context, _stateMachine));
            _stateMachine.AddState(new PlayerRollState(_context, _stateMachine));
            
            _stateMachine.SwitchState<PlayerIdleState>();

            CharacterController cc = GetComponent<CharacterController>();
            _footstepAudio = new FootstepAudioSystem(
            audioService, 
            transform, 
            _animEvents, 
            footstepConfig
            );
        }

        public void TakeDamage(float amount)
        {
            Health?.TakeDamage(amount);
            _audioService?.PlaySFX("Player_Hurt", transform.position);
            Debug.Log($"<color=green>[Player]</color> Получил {amount} урона. ХП: {Health.CurrentHealth}");
        }

        private void HandleDeath()
        {
            Debug.Log("<color=green>[Player]</color> ИГРОК МЕРТВ!");
            
            // Блокируем управление
            if (_inputService != null)
            {
                _inputService.OnJump -= OnJump;
                _inputService.OnReload -= OnReload;
                _inputService.OnMelee -= OnMelee;
            }
            
            // Отключаем физику, чтобы капсула не двигалась
            GetComponent<CharacterController>().enabled = false;
        }

        private void OnEnable()
        {
            if (_inputService != null)
            {
                _inputService.OnJump += OnJump;
                _inputService.OnReload += OnReload;
                _inputService.OnMelee += OnMelee; // Подписка на рукопашку!
            }
        }

        private void OnDisable()
        {
            if (_inputService != null)
            {
                _inputService.OnJump -= OnJump;
                _inputService.OnReload -= OnReload;
                _inputService.OnMelee -= OnMelee;
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            
            // Если игрок жив - обновляем физику, сенсоры и стейт-машину
            if (Health != null && !Health.IsDead)
            {
                _context.GroundSensor.Tick(); 
                _interactionSensor?.Tick();
                _stateMachine?.Tick(deltaTime);

            }

            // Оружие и камера обновляются независимо (камера может крутиться после смерти)
            _weaponController?.Tick(deltaTime);
            _cameraController?.Tick(deltaTime);
        }

        private void OnDestroy()
        {
            _weaponInventory?.Dispose();
            _footstepAudio?.Dispose();
            if (_animEvents != null) _animEvents.OnMeleeStrike -= _meleeController.PerformStrike;
            if (Health != null) Health.OnDeath -= HandleDeath; // Отписка
        }

        private void OnJump() => _stateMachine.HandleJump();
        
        private void OnReload()
        {
            if (_weaponInventory != null && _weaponInventory.CurrentWeapon != null)
                _weaponInventory.CurrentWeapon.Reload();
        }

        private void OnMelee() => _meleeController?.TryMeleeAttack(); // Вызов удара
    }
}