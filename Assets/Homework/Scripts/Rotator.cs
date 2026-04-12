using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netologia.Homework
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _rotate;

        private IEnumerator Start()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            while (true)
            {
                Quaternion deltaRotation = Quaternion.Euler(_rotate * Time.deltaTime);
                rb.MoveRotation(rb.rotation * deltaRotation);
                yield return new WaitForFixedUpdate();
            }
        }
    }
}


