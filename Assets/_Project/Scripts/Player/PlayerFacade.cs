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
using TpsShooter.Core;


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
        
        private PlayerMeleeController _meleeController;
        private CharacterAnimationEvents _animEvents;
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
            
            PlayerRigController rigController = new PlayerRigController(
                animator, _leftHandIkTarget, _weaponRig, _leftHandIK);

            _weaponController = new PlayerWeaponController(
                inputService, camTransform, _aimTarget, rigController);

            WeaponTransitionService transitionService = new WeaponTransitionService();

            _weaponInventory = new WeaponInventory(
                _weaponController, transitionService, inputService, transform, 
                _weaponHandSocket, _weaponBackSocket1, _weaponBackSocket2,
                lootFactory);

            _interactionSensor = new PlayerInteractionSensor(
                    transform, 
                    config.InteractionRadius, 
                    config.InteractableMask
                );

            InventoryModel = inventoryModel;

            Health = new HealthEngine(config.MaxHealth);
            Health.OnDeath += HandleDeath;


            _meleeController = new PlayerMeleeController(animator, transform, config);
            
            _animEvents = animator.gameObject.GetComponent<CharacterAnimationEvents>();
            if (_animEvents == null) 
                _animEvents = animator.gameObject.AddComponent<CharacterAnimationEvents>();
                
            _animEvents.OnMeleeStrike += _meleeController.PerformStrike;


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
            DevLogger.Log($"<color=green>[Player]</color> Получил {amount} урона. ХП: {Health.CurrentHealth}");
        }

        private void HandleDeath()
        {
            DevLogger.Log("<color=green>[Player]</color> ИГРОК МЕРТВ!");
            
            if (_inputService != null)
            {
                _weaponInventory?.CancelAllOperations();
                _inputService.OnJump -= OnJump;
                _inputService.OnReload -= OnReload;
                _inputService.OnMelee -= OnMelee;
            }
            
            _context.Controller.enabled = false;
        }

        private void OnEnable()
        {
            if (_inputService != null)
            {
                _inputService.OnJump += OnJump;
                _inputService.OnReload += OnReload;
                _inputService.OnMelee += OnMelee;
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
            
            if (Health != null && !Health.IsDead)
            {
                _context.GroundSensor.Tick(); 
                _interactionSensor?.Tick();
                _stateMachine?.Tick(deltaTime);

                // ИСПРАВЛЕНИЕ: Перенесли обновление пушки и камеры сюда.
                // Теперь мертвый игрок не может ни стрелять, ни крутить головой.
                _weaponController?.Tick(deltaTime);
                _cameraController?.Tick(deltaTime);
            }
        }

        private void OnDestroy()
        {
            _weaponInventory?.Dispose();
            _footstepAudio?.Dispose();
            if (_animEvents != null) _animEvents.OnMeleeStrike -= _meleeController.PerformStrike;
            if (Health != null) Health.OnDeath -= HandleDeath;
        }

        private void OnJump() => _stateMachine.HandleJump();
        
        private void OnReload()
        {
            if (_weaponInventory != null && _weaponInventory.CurrentWeapon != null)
                _weaponInventory.CurrentWeapon.Reload();
        }

        private void OnMelee() => _meleeController?.TryMeleeAttack();
    }
}