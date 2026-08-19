using UnityEngine;

namespace TpsShooter.Enemies.Core
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimator : MonoBehaviour
    {
        private Animator _animator;

        private static readonly int IdleHash = Animator.StringToHash("Idle");
        private static readonly int WalkHash = Animator.StringToHash("Walk");
        private static readonly int RunHash = Animator.StringToHash("Run");
        
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int DeathHash = Animator.StringToHash("Death");

        private const float DefaultCrossfade = 0.2f;
        private const float FastCrossfade = 0.05f;
        private const float AlertCrossfade = 0.1f;
        private const float DeathLockTime = 10f;
        private const float HitLockTime = 0.3f;

        private int _currentLoopingAnim = IdleHash;
        private int _lastPlayedHash = 0; 
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

        public void PlayIdle() { _currentLoopingAnim = IdleHash; PlayLooping(IdleHash); }
        public void PlayWalk() { _currentLoopingAnim = WalkHash; PlayLooping(WalkHash); }
        public void PlayRun()  { _currentLoopingAnim = RunHash; PlayLooping(RunHash); }

        public void PlayCustomIdle(string stateName)
        {
            int hash = Animator.StringToHash(stateName);
            _currentLoopingAnim = hash;
            PlayLooping(hash);
        }

        public void PlayCustomLooping(string stateName)
        {
            int hash = Animator.StringToHash(stateName);
            _currentLoopingAnim = hash;
            PlayLooping(hash);
        }

        private void PlayLooping(int hash)
        {
            if (_lockTime <= 0) 
            {
                if (_lastPlayedHash != hash)
                {
                    _animator.CrossFadeInFixedTime(hash, DefaultCrossfade);
                    _lastPlayedHash = hash;
                }
            }
        }

        public void PlayAttack(string stateName, float duration)
        {
            PlayOneShot(Animator.StringToHash(stateName), duration, FastCrossfade);
        }
        
        public void PlayAlert(string stateName, float duration)
        {
            PlayOneShot(Animator.StringToHash(stateName), duration, AlertCrossfade);
        }

        public void PlayHit() 
        { 
            if (_lockTime > 0) return; 

            PlayOneShot(HitHash, HitLockTime, FastCrossfade);
        }
        
        public void PlayDeath() 
        { 
            _currentLoopingAnim = DeathHash; 
            PlayOneShot(DeathHash, DeathLockTime, AlertCrossfade);
        }

        private void PlayOneShot(int hash, float lockDuration, float transitionTime)
        {
            _animator.CrossFadeInFixedTime(hash, transitionTime);
            _lastPlayedHash = hash; 
            _lockTime = lockDuration;
        }
    }
}