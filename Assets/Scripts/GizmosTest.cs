using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmosTest : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        // Gizmos.DrawSphere(Vector3.zero, 4f);
        // Gizmos.DrawWireSphere(Vector3.zero, 4f);
        // Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.DrawRay(transform.position, Vector3.up);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(Vector3.zero, 7f);
    }

}
