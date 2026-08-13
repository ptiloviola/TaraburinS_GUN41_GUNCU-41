using UnityEngine;

namespace TpsShooter.Enemies.Core
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimator : MonoBehaviour
    {
        private Animator _animator;

        // Хэши цикличных анимаций (передвижение)
        private static readonly int IdleHash = Animator.StringToHash("Idle");
        private static readonly int WalkHash = Animator.StringToHash("Walk");
        private static readonly int RunHash = Animator.StringToHash("Run");
        
        // Хэши одноразовых анимаций (действия)
        private static readonly int ShootHash = Animator.StringToHash("Firing Rifle");
        private static readonly int MeleeAttackHash = Animator.StringToHash("Attack");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int DeathHash = Animator.StringToHash("Death");

        private int _currentLoopingAnim = IdleHash;
        private float _lockTime = 0f;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            // Система возврата к цикличному движению после выстрела или получения урона
            if (_lockTime > 0)
            {
                _lockTime -= Time.deltaTime;
                if (_lockTime <= 0 && _currentLoopingAnim != DeathHash)
                {
                    _animator.CrossFadeInFixedTime(_currentLoopingAnim, 0.25f);
                }
            }
        }

        // --- ЦИКЛИЧНЫЕ (Обновляют запомненное состояние) ---
        public void PlayIdle() { _currentLoopingAnim = IdleHash; PlayLooping(IdleHash); }
        public void PlayWalk() { _currentLoopingAnim = WalkHash; PlayLooping(WalkHash); }
        public void PlayRun()  { _currentLoopingAnim = RunHash; PlayLooping(RunHash); }

        private void PlayLooping(int hash)
        {
            // Играем, только если не заблокированы выстрелом/уроном
            if (_lockTime <= 0) 
                _animator.CrossFadeInFixedTime(hash, 0.2f);
        }

        // --- ОДНОРАЗОВЫЕ (Блокируют аниматор на время своего проигрывания) ---
        public void PlayShoot() 
        { 
            _animator.CrossFadeInFixedTime(ShootHash, 0.05f); 
            _lockTime = 0.4f; // Возврат в Idle/Run через 0.4 сек
        }
        
        public void PlayMeleeAttack() 
        { 
            _animator.CrossFadeInFixedTime(MeleeAttackHash, 0.05f); 
            _lockTime = 0.8f; // Удар длится дольше
        }
        
        public void PlayHit() 
        { 
            _animator.CrossFadeInFixedTime(HitHash, 0.05f); 
            _lockTime = 0.3f; // Быстрое вздрагивание
        }
        
        public void PlayDeath() 
        { 
            _currentLoopingAnim = DeathHash; // Чтобы не воскрес случайно
            _animator.CrossFadeInFixedTime(DeathHash, 0.1f); 
            _lockTime = 999f; 
        }
    }
}