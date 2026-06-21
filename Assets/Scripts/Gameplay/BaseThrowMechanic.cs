
using System;
using UnityEngine;


namespace Bowling.Gameplay
{
    public abstract class BaseThrowMechanic : MonoBehaviour
    {
        public Action<Vector3, float> OnThrowExecuted;
        protected BowlingInputActions _inputActions;
        protected bool _isThrowExecuted = false;
        [SerializeField] protected bool _isPointerOverUI = false;

        public virtual void Initialize(BowlingInputActions inputActions)
        {
            _inputActions = inputActions;
        }
        protected void ExecuteThrow(Vector3 dir, float force)
        {
            _isThrowExecuted = true;
            OnThrowExecuted?.Invoke(dir, force);
        }
        public virtual void ResetMechanic()
        {
            _isThrowExecuted = false;
        }
        public abstract void EnableMechanic();
        public abstract void DisableMechanic();

        // //наверное, вызывать в апдейте не очень хорошо, но это помогло починить варнинг 
        // // "Calling IsPointerOverGameObject() 
        // // from within event processing (such as from InputAction callbacks) 
        // // will not work as expected; it will query UI state from the last frame", 
        // // появившийся на новой системе ввода. 
        // // Решилось переносом проверки нахождения над элементом UI из механики инпута сюда
        protected virtual void Update()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                _isPointerOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            }
        }

    }
}

