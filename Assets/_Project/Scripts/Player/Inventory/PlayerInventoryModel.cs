using System;
using System.Collections.Generic;

namespace TpsShooter.Player.Inventory
{
    // Простой enum для типов патронов. Позже можно вынести в отдельный файл.
    public enum AmmoType 
    { 
        Pistol, 
        Rifle, 
        Shotgun 
    }

    public class PlayerInventoryModel
    {
        // --- ЗДОРОВЬЕ ---
        public float MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }

        // --- ПАТРОНЫ ---
        private readonly Dictionary<AmmoType, int> _ammo = new Dictionary<AmmoType, int>();
        private readonly Dictionary<AmmoType, int> _maxAmmo = new Dictionary<AmmoType, int>();

        // --- СОБЫТИЯ ДЛЯ UI (OBSERVER) ---
        public event Action<float, float> OnHealthChanged; // Current, Max
        public event Action<AmmoType, int> OnAmmoChanged;  // Type, CurrentAmount

        public PlayerInventoryModel(float maxHealth, float startingHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = startingHealth;
        }

        // --- ЛОГИКА АПТЕЧЕК ---
        public bool TryHeal(float amount)
        {
            if (CurrentHealth >= MaxHealth) return false; // Здоровье уже полное, не подбираем

            CurrentHealth += amount;
            if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;

            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            return true;
        }

        // --- ЛОГИКА ПАТРОНОВ ---
        public void InitializeAmmoCapacity(AmmoType type, int maxCapacity)
        {
            if (!_ammo.ContainsKey(type))
            {
                _ammo[type] = 0;
                _maxAmmo[type] = maxCapacity;
            }
        }

        public bool TryAddAmmo(AmmoType type, int amount)
        {
            if (!_ammo.ContainsKey(type)) return false; // Если такой тип не инициализирован
            if (_ammo[type] >= _maxAmmo[type]) return false; // Карманы полны

            _ammo[type] += amount;
            if (_ammo[type] > _maxAmmo[type]) _ammo[type] = _maxAmmo[type];

            OnAmmoChanged?.Invoke(type, _ammo[type]);
            return true;
        }
    }
}