using System;
using UnityEngine;

namespace Bowling.Gameplay
{
    public abstract class BaseThrowMechanic : MonoBehaviour
    {
        public event Action<Vector3, float> OnThrowExecuted; 
        
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

        protected virtual void Update()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                _isPointerOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            }
        }
    }
}