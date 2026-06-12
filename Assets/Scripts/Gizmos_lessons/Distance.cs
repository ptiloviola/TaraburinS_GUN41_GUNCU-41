using UnityEngine;
using TMPro;

public class Distance : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    [SerializeField] private float _minDist = 10f;
    [SerializeField] private TextMeshProUGUI _distDisplay;
    private float _distToPlayer;

    private Vector3 _enemyPosition;

    private Vector3 _directionToPlayer;

    void Start()
    {
        _enemyPosition = transform.position;
        
    }


    void Update()
    {
        _directionToPlayer = _player.transform.position - _enemyPosition;
        _distToPlayer = _directionToPlayer.magnitude;

        _distDisplay.text = _distToPlayer.ToString("0.0") + "m";
        if (_distToPlayer < _minDist)
        {
            _distDisplay.color = Color.red;
        }
        else
        {
            _distDisplay.color = Color.yellow;
        }
        
    }
}
