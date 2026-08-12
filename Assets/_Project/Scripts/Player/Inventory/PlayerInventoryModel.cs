using System;
using System.Collections.Generic;
using TpsShooter.Player.Configs; // Для доступа к PlayerInventoryConfig
using Zenject; // Для IInitializable

namespace TpsShooter.Player.Inventory
{
    public enum AmmoType { Pistol, Rifle, Shotgun }

    // Реализуем IInitializable, чтобы Zenject сам вызвал настройку при старте
    public class PlayerInventoryModel : IInitializable
    {
        public float MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }

        private readonly Dictionary<AmmoType, int> _ammo = new Dictionary<AmmoType, int>();
        private readonly Dictionary<AmmoType, int> _maxAmmo = new Dictionary<AmmoType, int>();
        private readonly PlayerInventoryConfig _config;

        public event Action<float, float> OnHealthChanged;
        public event Action<AmmoType, int> OnAmmoChanged;

        // Zenject автоматически передаст конфиг при создании
        public PlayerInventoryModel(PlayerInventoryConfig config)
        {
            _config = config;
        }

        // Zenject вызовет это при старте сцены
        public void Initialize()
        {
            MaxHealth = _config.MaxHealth;
            CurrentHealth = _config.StartingHealth;

            foreach (var limit in _config.AmmoLimits)
            {
                _ammo[limit.Type] = 0;
                _maxAmmo[limit.Type] = limit.MaxCapacity;
            }
        }

        public bool TryHeal(float amount)
        {
            if (CurrentHealth >= MaxHealth) return false;
            CurrentHealth = Math.Min(CurrentHealth + amount, MaxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            return true;
        }

        public bool TryAddAmmo(AmmoType type, int amount)
        {
            if (!_ammo.ContainsKey(type)) return false;
            if (_ammo[type] >= _maxAmmo[type]) return false;

            _ammo[type] = Math.Min(_ammo[type] + amount, _maxAmmo[type]);
            OnAmmoChanged?.Invoke(type, _ammo[type]);
            return true;
        }

        // --- НОВЫЙ МЕТОД ДЛЯ ПЕРЕЗАРЯДКИ ОРУЖИЯ ---
        public int ConsumeAmmo(AmmoType type, int amountNeeded)
        {
            if (!_ammo.ContainsKey(type) || _ammo[type] <= 0) return 0;

            int amountToTake = Math.Min(_ammo[type], amountNeeded);
            _ammo[type] -= amountToTake;
            OnAmmoChanged?.Invoke(type, _ammo[type]);
            
            return amountToTake; // Возвращаем, сколько реально патронов смогли дать
        }

        public int GetAmmo(AmmoType type) => _ammo.ContainsKey(type) ? _ammo[type] : 0;
    }
}