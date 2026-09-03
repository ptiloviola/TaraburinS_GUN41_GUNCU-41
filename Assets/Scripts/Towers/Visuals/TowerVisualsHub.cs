using UnityEngine;
using System;
using Gameplay.Towers.Behaviors;

namespace Gameplay.Towers.Visuals
{
    // Вешаем на корень префаба башни
    public class TowerVisualsHub : MonoBehaviour, ITowerVisuals
    {
        // --- РЕАЛИЗАЦИЯ ИНТЕРФЕЙСА ITowerVisuals ---
        public event Action OnAttackImpact;

        // --- ЛОКАЛЬНЫЕ СОБЫТИЯ ДЛЯ КОМПОНЕНТОВ ПРЕФАБА ---
        // (На них подписываются наши VisualRotator, MultiBarrelAnimator и т.д.)
        public event Action OnBuild;
        public event Action<Vector3> OnShoot;

        private WeaponAdapter _attackBehavior;

        private void Awake()
        {
            // Пытаемся найти логику, если она есть на этом же объекте или родителе
            _attackBehavior = GetComponentInParent<WeaponAdapter>();
        }

        private void OnEnable()
        {
            if (_attackBehavior != null)
            {
                // Подписываемся на события от логики
                _attackBehavior.OnBuildStarted += PlayBuildAnimation;
                _attackBehavior.OnShotFired += PlayShootAnimation;
            }
        }

        private void OnDisable()
        {
            if (_attackBehavior != null)
            {
                _attackBehavior.OnBuildStarted -= PlayBuildAnimation;
                _attackBehavior.OnShotFired -= PlayShootAnimation;
            }
        }

        // --- РЕАЛИЗАЦИЯ МЕТОДОВ ИНТЕРФЕЙСА ---

        public void Initialize()
        {
            // Здесь в будущем мы сможем сбрасывать состояние компонентов, 
            // если башня была уничтожена и снова взята из Object Pool'а.
            // Например: восстанавливать цвет, сбрасывать партиклы и т.д.
        }

        public void PlayBuildAnimation()
        {
            // Транслируем команду всем визуальным компонентам на префабе
            OnBuild?.Invoke();
        }

        public void PlayShootAnimation(Vector3 targetPosition)
        {
            // Транслируем координаты выстрела компонентам (стволам, лазерам)
            OnShoot?.Invoke(targetPosition);
        }

        // --- ДОПОЛНИТЕЛЬНЫЙ МЕТОД ---
        
        // Этот метод мы сможем вызывать из Animation Events внутри Unity Animator.
        // Например, мечник замахивается, и на нужном кадре анимации дергает этот метод,
        // а Хаб передает это событие в боевую логику.
        public void TriggerAttackImpact()
        {
            OnAttackImpact?.Invoke();
        }
    }
}