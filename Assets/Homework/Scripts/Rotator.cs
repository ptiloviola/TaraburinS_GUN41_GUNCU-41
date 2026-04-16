using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netologia.Homework
{
    [RequireComponent(typeof(Rigidbody))]
    public class Rotator : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _rotate;

        private WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();

        private IEnumerator Start()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            while (true)
            {
                Quaternion deltaRotation = Quaternion.Euler(_rotate * Time.deltaTime);
                rb.MoveRotation(rb.rotation * deltaRotation);
                yield return _waitForFixedUpdate;
            }
        }
    }
}


