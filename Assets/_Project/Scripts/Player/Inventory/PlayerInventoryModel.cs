using System;
using System.Collections.Generic;
using TpsShooter.Player.Configs;
using Zenject; 

namespace TpsShooter.Player.Inventory
{
    public enum AmmoType { Pistol, Rifle, Shotgun }

    public class PlayerInventoryModel : IInitializable
    {
        private readonly Dictionary<AmmoType, int> _ammo = new Dictionary<AmmoType, int>();
        private readonly Dictionary<AmmoType, int> _maxAmmo = new Dictionary<AmmoType, int>();
        private readonly PlayerInventoryConfig _config;

        public event Action<AmmoType, int> OnAmmoChanged;

        public PlayerInventoryModel(PlayerInventoryConfig config)
        {
            _config = config;
        }

        public void Initialize()
        {
            foreach (var limit in _config.AmmoLimits)
            {
                _ammo[limit.Type] = 0;
                _maxAmmo[limit.Type] = limit.MaxCapacity;
            }
        }

        public bool TryAddAmmo(AmmoType type, int amount)
        {
            if (!_ammo.ContainsKey(type)) return false;
            if (_ammo[type] >= _maxAmmo[type]) return false;

            _ammo[type] = Math.Min(_ammo[type] + amount, _maxAmmo[type]);
            OnAmmoChanged?.Invoke(type, _ammo[type]);
            return true;
        }

        public int ConsumeAmmo(AmmoType type, int amountNeeded)
        {
            if (!_ammo.ContainsKey(type) || _ammo[type] <= 0) return 0;

            int amountToTake = Math.Min(_ammo[type], amountNeeded);
            _ammo[type] -= amountToTake;
            OnAmmoChanged?.Invoke(type, _ammo[type]);
            
            return amountToTake; 
        }

        public int GetAmmo(AmmoType type) => _ammo.ContainsKey(type) ? _ammo[type] : 0;
    }
}