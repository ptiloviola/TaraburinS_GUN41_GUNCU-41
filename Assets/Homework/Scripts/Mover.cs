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

        [ContextMenu("1. Записать текущую позицию как _start")]
        private void SetStartToCurrent()
        {
            _start = transform.position;
            Debug.Log("Стартовая точка обновлена!");
        }
        [ContextMenu("2. Записать текущую позицию как _end")]
        private void SetEndToCurrent()
        {
            _end = transform.position;
            Debug.Log("Конечная точка обновлена!");
        }

    }
}



