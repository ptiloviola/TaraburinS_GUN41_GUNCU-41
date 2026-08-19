using UnityEngine;

namespace TpsShooter.Enemies.Core
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimator : MonoBehaviour
    {
        private Animator _animator;

        // Возвращаем простые, надежные хэши
        private static readonly int IdleHash = Animator.StringToHash("Idle");
        private static readonly int WalkHash = Animator.StringToHash("Walk");
        private static readonly int RunHash = Animator.StringToHash("Run");
        
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int DeathHash = Animator.StringToHash("Death");

        private int _currentLoopingAnim = IdleHash;
        private int _lastPlayedHash = 0; // Наша защита от дрожи осталась!
        private float _lockTime = 0f;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (_lockTime > 0)
            {
                _lockTime -= Time.deltaTime;
                if (_lockTime <= 0 && _currentLoopingAnim != DeathHash)
                {
                    PlayLooping(_currentLoopingAnim);
                }
            }
        }

        // --- ЦИКЛИЧНЫЕ (Прямые вызовы стейтов) ---
        public void PlayIdle() { _currentLoopingAnim = IdleHash; PlayLooping(IdleHash); }
        public void PlayWalk() { _currentLoopingAnim = WalkHash; PlayLooping(WalkHash); }
        public void PlayRun()  { _currentLoopingAnim = RunHash; PlayLooping(RunHash); }

        public void PlayCustomIdle(string stateName)
        {
            int hash = Animator.StringToHash(stateName);
            _currentLoopingAnim = hash;
            PlayLooping(hash);
        }

        private void PlayLooping(int hash)
        {
            if (_lockTime <= 0) 
            {
                // Если мы уже играем эту анимацию - не дергаем Аниматор
                if (_lastPlayedHash != hash)
                {
                    _animator.CrossFadeInFixedTime(hash, 0.2f);
                    _lastPlayedHash = hash;
                }
            }
        }

        // --- ОДНОРАЗОВЫЕ (Блокируют аниматор) ---
        public void PlayCustomMelee(string stateName, float duration = 0.8f)
        {
            PlayOneShot(Animator.StringToHash(stateName), duration, 0.05f);
        }
        
        public void PlayAlert(string stateName, float duration = 1.0f)
        {
            PlayOneShot(Animator.StringToHash(stateName), duration, 0.1f);
        }

        public void PlayHit() 
        { 
            // ИСПРАВЛЕНИЕ: Если враг сейчас бьет или рычит (_lockTime > 0) — 
            // пули наносят урон, но НЕ прерывают его анимацию! Это уберет судороги.
            if (_lockTime > 0) return; 

            PlayOneShot(HitHash, 0.3f, 0.05f);
        }
        
        public void PlayDeath() 
        { 
            _currentLoopingAnim = DeathHash; 
            PlayOneShot(DeathHash, 999f, 0.1f);
        }

        public void PlayShoot() // Оставили для стрелков
        {
            PlayOneShot(Animator.StringToHash("Firing Rifle"), 0.4f, 0.05f);
        }

        private void PlayOneShot(int hash, float lockDuration, float transitionTime)
        {
            _animator.CrossFadeInFixedTime(hash, transitionTime);
            _lastPlayedHash = hash; 
            _lockTime = lockDuration;
        }
    }
}