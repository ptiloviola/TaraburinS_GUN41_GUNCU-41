using UnityEngine;
using MeatMushrooms.Player.Components;
using System;

namespace MeatMushrooms.Player
{
    public class PlayerRegistry
    {
        public PlayerController Controller { get; private set; }
        public PlayerHealth Health { get; private set; }
        public PlayerStealth Stealth { get; private set; }
        
        public event Action OnPlayerSpawned;
        public void Register(GameObject playerInstance)
        {
            Controller = playerInstance.GetComponent<PlayerController>();
            Health = playerInstance.GetComponent<PlayerHealth>();
            Stealth = playerInstance.GetComponent<PlayerStealth>();
            
            OnPlayerSpawned?.Invoke(); 
        }
    }
}