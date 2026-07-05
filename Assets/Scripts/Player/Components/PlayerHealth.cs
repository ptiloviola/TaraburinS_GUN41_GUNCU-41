using System;
using UnityEngine;

namespace MeatMushrooms.Player.Components
{
    public class PlayerHealth : MonoBehaviour
    {
        public event Action OnDeath;
        public bool IsDead { get; private set; }

        public void Kill()
        {
            if (IsDead) return;
            IsDead = true;
            
            Debug.Log("<color=red><b>ВЫ МЕРТВЫ! Игра окончена.</b></color>");
            OnDeath?.Invoke(); 
        }
    }
}