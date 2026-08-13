using System;
using Zenject;
using TpsShooter.Player.Inventory;
using TpsShooter.UI;
using TpsShooter.Player; // Для PlayerFacade и доступа к текущему оружию

namespace TpsShooter.UI.Presenters
{
    public class PlayerHUDPresenter : IInitializable, IDisposable, ITickable
    {
        private readonly PlayerInventoryModel _model;
        private readonly PlayerHUDView _view;
        private readonly PlayerFacade _player;

        public PlayerHUDPresenter(PlayerInventoryModel model, PlayerHUDView view, PlayerFacade player)
        {
            _model = model;
            _view = view;
            _player = player;
        }

        public void Initialize()
        {
            // Подписываемся на события модели
            _player.Health.OnHealthChanged += HandleHealthChanged;
            _model.OnAmmoChanged += HandleAmmoChanged;

            // Обновляем UI начальными значениями
            HandleHealthChanged(_player.Health.CurrentHealth, _player.Health.MaxHealth);
        }

        public void Dispose()
        {
            _player.Health.OnHealthChanged -= HandleHealthChanged;
            _model.OnAmmoChanged -= HandleAmmoChanged;
        }

        // Вызывается каждый кадр Zenject'ом. Нужно только для обновления магазина текущей пушки.
        // Запас патронов мы обновляем по событиям.
        public void Tick()
        {
            UpdateWeaponUI();
        }

        private void HandleHealthChanged(float current, float max) => _view.UpdateHealth(current, max);
        
        private void HandleAmmoChanged(AmmoType type, int amount) => UpdateWeaponUI();

        private void UpdateWeaponUI()
        {
            var currentWeapon = _player.WeaponInventory?.CurrentWeapon;
            if (currentWeapon != null)
            {
                int reserve = _model.GetAmmo(currentWeapon.Config.WeaponAmmoType);
                _view.UpdateAmmo(currentWeapon.CurrentAmmoInClip, reserve);
            }
            else
            {
                _view.UpdateAmmo(0, 0);
            }
        }
    }
}