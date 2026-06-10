using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTest : MonoBehaviour
{
    private float _timer;
    private int _level;
    [SerializeField] private int _allCountLevel;

    private void Start()
    {
        print("до паузы");
        Debug.Break();
        print("после паузы");
    }


    private void Update()
    {
        // GetLevel(42);
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     Debug.DrawLine(Vector3.zero, new Vector3(5, 4, 0), Color.cyan, 2.5f);
        // }

        // Debug.DrawRay(new Vector3(-1, 0, 0), new Vector3(5, 4, 0), Color.red);

        Debug.DrawRay(Vector3.zero, Vector3.up * 3, Color.red);
        Debug.DrawRay(transform.position, Vector3.up * 3, Color.yellow);
        Debug.DrawRay(Vector3.zero, transform.up * 3, Color.green);


        print("до паузы");
        Debug.Break();
        print("после паузы");

    
    }

    private void GetLevel(int level)
    {
        if (level < _allCountLevel && level > 0)
        {
            _level = level;
        }
        else 
        {
            Debug.LogWarning("Такого уровня нет");
            Debug.LogError("Такого уровня нет");
        }
    
    }
    

}
