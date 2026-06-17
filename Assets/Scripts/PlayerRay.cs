using UnityEngine;

public class PlayerRay : MonoBehaviour
{
    [SerializeField] private float _size = 10f;
    private void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        // ray.origin = Vector3.left;
        // ray.direction = Vector3.up;

        // Debug.DrawRay(transform.position, transform.forward * _size, Color.red);
        Debug.DrawRay(ray.origin, ray.direction * _size, Color.red);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log($"Hit {hit.collider.gameObject.name}");
        }
    }
}
