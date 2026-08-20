using System;
using Zenject;
using TpsShooter.Player.Inventory;
using TpsShooter.Player;
using TpsShooter.Weapons.Core; 

namespace TpsShooter.UI.Presenters
{
    public class PlayerHUDPresenter : IInitializable, IDisposable, ITickable
    {
        private readonly PlayerInventoryModel _model;
        private readonly PlayerHUDView _view;
        private readonly PlayerFacade _player;

        private int _lastClipAmmo = -1;
        private int _lastReserveAmmo = -1;
        private WeaponBase _lastWeapon;

        public PlayerHUDPresenter(PlayerInventoryModel model, PlayerHUDView view, PlayerFacade player)
        {
            _model = model;
            _view = view;
            _player = player;
        }

        public void Initialize()
        {
            _player.Health.OnHealthChanged += HandleHealthChanged;
            _model.OnAmmoChanged += HandleAmmoChanged;

            HandleHealthChanged(_player.Health.CurrentHealth, _player.Health.MaxHealth);
            ForceUpdateWeaponUI();
        }

        public void Dispose()
        {
            _player.Health.OnHealthChanged -= HandleHealthChanged;
            _model.OnAmmoChanged -= HandleAmmoChanged;
        }

        public void Tick()
        {
            var currentWeapon = _player.WeaponInventory?.CurrentWeapon;
            
            if (currentWeapon != null)
            {
                int currentClip = currentWeapon.CurrentAmmoInClip;
                int reserve = _model.GetAmmo(currentWeapon.Config.WeaponAmmoType);

                if (currentClip != _lastClipAmmo || reserve != _lastReserveAmmo || currentWeapon != _lastWeapon)
                {
                    _lastClipAmmo = currentClip;
                    _lastReserveAmmo = reserve;
                    _lastWeapon = currentWeapon;
                    
                    _view.UpdateAmmo(currentClip, reserve);
                }
            }
            else 
            {
                if (_lastWeapon != null || _lastClipAmmo == -1) 
                {
                    _lastWeapon = null;
                    _lastClipAmmo = -2;
                    _lastReserveAmmo = -1;
                    
                    _view.UpdateAmmo(0, 0);
                }
            }
        }

        private void HandleHealthChanged(float current, float max) => _view.UpdateHealth(current, max);
        
        private void HandleAmmoChanged(AmmoType type, int amount) => ForceUpdateWeaponUI();

        private void ForceUpdateWeaponUI()
        {
            _lastClipAmmo = -1; 
        }
    }
}