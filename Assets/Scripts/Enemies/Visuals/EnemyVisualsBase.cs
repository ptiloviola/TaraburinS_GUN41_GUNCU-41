using UnityEngine;
using Gameplay.Enemies.FSM;
using Cysharp.Threading.Tasks;

namespace Gameplay.Enemies.Visuals
{
    [RequireComponent(typeof(EnemyFacade))]
    public abstract class EnemyVisualsBase : MonoBehaviour
    {
        protected EnemyFacade Facade { get; private set; }

        protected virtual void Awake()
        {
            Facade = GetComponent<EnemyFacade>();
        }

        protected virtual void OnEnable()
        {
            Facade.OnStateChanged += HandleStateChanged;
        }

        protected virtual void OnDisable()
        {
            Facade.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(EnemyStateType state)
        {
            switch (state)
            {
                case EnemyStateType.Move: OnMoveStart(); break;
                case EnemyStateType.Stunned: OnStunned(); break;
                case EnemyStateType.ReachedBase: OnReachedBase(); break;
            }
        }

        // Виртуальные методы для наследников
        protected virtual void OnMoveStart() { }
        protected virtual void OnStunned() { }
        protected virtual void OnReachedBase() { }
        public virtual async UniTask PlayDeathAnimationAsync()
        {
            // Базовая реализация: просто ждем 0 секунд.
            // В наследниках (например, JumperAnimator) ты переопределишь этот метод,
            // запустишь анимацию рассыпания и напишешь: await UniTask.Delay(1000);
            await UniTask.Yield(); 
        }
    }
}