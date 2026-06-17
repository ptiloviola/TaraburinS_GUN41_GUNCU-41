
using System;
using UnityEngine;


namespace Bowling.UI
{
    public class BaseThrowMechanic : MonoBehaviour
    {
        protected bool _isThrowExecuted = false;
        public Action<Vector3, float> OnThrowExecuted;
        protected void ExecuteThrow(Vector3 dir, float force)
        {
            _isThrowExecuted = true;
            OnThrowExecuted?.Invoke(dir, force);
        }
        public virtual void ResetMechanic()
        {
            _isThrowExecuted = false;
        }
    }
}

