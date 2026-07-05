using System;
using UnityEngine;
using MeatMushrooms.Player.Components;

namespace MeatMushrooms.Environment
{
    public class ExitZone : MonoBehaviour
    {
        public static event Action OnLevelCompleted; 

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerController>() != null)
            {
                Debug.Log("<color=green>[Exit]</color> Шапочка спаслась!");
                OnLevelCompleted?.Invoke();
            }
        }
    }
}