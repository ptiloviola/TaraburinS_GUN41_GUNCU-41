using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Gates : MonoBehaviour
{
    private int _score = 0;

    private void OnTriggerEnter(Collider other)
    {
        //Вопрос: или лучше использовать проверку other.GetComponent<Ball>() != null? Или нет разницы на практике?
        if(other.TryGetComponent(out Ball ball))
        {
            _score ++;
            Debug.Log($"GOOOOOL!!! Score = {_score}");
            Destroy(other.gameObject);
        }
    }
}
