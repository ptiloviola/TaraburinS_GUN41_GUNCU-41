using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Netologia.Homework
{
    public class Mover : MonoBehaviour
    {

        [SerializeField]
        private Vector3 _start;
        [SerializeField]
        private Vector3 _end;
        [SerializeField]
        private float _speed;
        [SerializeField]
        private float _delay;
        
        // Вопрос: я использовал Vector3.Lerp т.к. в лекциях показывали этот метод для лучшей реализации перемещений. 
        // Но мне кажется, использование Vector3.MoveTowards было бы проще. Как на практике реализуют перемещение для подобной задачи?
        private IEnumerator Start()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.position = _start;
            float distance = Vector3.Distance(_start, _end);
            var progress = 0f;
            while(true)
            {
                if (progress < 1f)
                {
                    progress += (_speed/distance)*Time.deltaTime;
                    Vector3 nextPosition = Vector3.Lerp(_start, _end, progress);
                    rb.MovePosition(nextPosition);
                }
                else
                {
                    yield return new WaitForSeconds(_delay);
                    (_start, _end) = (_end, _start);
                    progress = 0f;
                }
                yield return new WaitForFixedUpdate();
            }
        }

        [ContextMenu("1. Set current position as _start")]
        private void SetStartToCurrent()
        {
            _start = transform.position;
            Debug.Log("Start position updated");
        }
        
        [ContextMenu("2. Set current position as _end")]
        private void SetEndToCurrent()
        {
            _end = transform.position;
            Debug.Log("End position updated");
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_start, 0.3f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_end, 0.3f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(_start, _end);
        }
        
    }
}



