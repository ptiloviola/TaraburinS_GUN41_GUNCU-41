
using System;
using UnityEngine;


namespace Bowling.UI
{
    public class BaseThrowMechanic : MonoBehaviour
    {
        public Action<Vector3, float> OnThrowExecuted;
        protected void ExecuteThrow(Vector3 dir, float force)
        {
            OnThrowExecuted?.Invoke(dir, force);
        }
    }
}

