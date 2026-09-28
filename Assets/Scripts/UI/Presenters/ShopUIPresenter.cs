using System;
using Zenject;
using Gameplay.UI.Views;
using Gameplay.Campaign.Data;
using Gameplay.Modifiers.Data;

namespace Gameplay.UI.Presenters
{
    public class ShopUIPresenter : IInitializable, IDisposable
    {
        private readonly ShopUIView _view;
        private readonly RunProgressModel _progressModel;

        public event Action OnShopClosed;

        private ShopConfig _currentConfig;

        public ShopUIPresenter(ShopUIView view, RunProgressModel progressModel)
        {
            _view = view;
            _progressModel = progressModel;
        }

        public void Initialize()
        {
            _view.OnItemBuyClicked += HandleBuyItem;
            _view.OnLeaveClicked += HandleLeaveShop;
            _view.HideShop();
        }

        public void Dispose()
        {
            _view.OnItemBuyClicked -= HandleBuyItem;
            _view.OnLeaveClicked -= HandleLeaveShop;
        }

        public void OpenShop(ShopConfig config)
        {
            _currentConfig = config;
            _view.ShowShop(_currentConfig, _progressModel.CurrentRunGold);
        }

        private void HandleBuyItem(ItemConfig item, ShopItemView itemView)
        {
            if (_progressModel.CurrentRunGold >= item.BaseCost)
            {
                _progressModel.AddGold(-item.BaseCost); 
                _progressModel.AddItem(item.ItemId);

                UnityEngine.Debug.Log($"<color=green>[Shop] Куплен предмет: {item.DisplayName}</color>");

                UnityEngine.Object.Destroy(itemView.gameObject);

                _view.RefreshAfterPurchase(_progressModel.CurrentRunGold);
            }
        }

        private void HandleLeaveShop()
        {
            _view.HideShop();
            OnShopClosed?.Invoke();
        }
    }
}