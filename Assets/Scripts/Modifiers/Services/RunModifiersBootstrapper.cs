using Zenject;
using Gameplay.Core.Data;
using Gameplay.Modifiers.Data;

namespace Gameplay.Modifiers.Services
{
    public class RunModifiersBootstrapper : IInitializable
    {
        private readonly StatsModifierService _modifierService;
        private readonly RunProgressModel _progressModel;
        private readonly ItemRegistry _itemRegistry;

        public RunModifiersBootstrapper(
            StatsModifierService modifierService, 
            RunProgressModel progressModel, 
            ItemRegistry itemRegistry)
        {
            _modifierService = modifierService;
            _progressModel = progressModel;
            _itemRegistry = itemRegistry;
        }

        public void Initialize()
        {
            if (_itemRegistry == null || _progressModel.ActiveRunItems == null) return;

            foreach (string itemId in _progressModel.ActiveRunItems)
            {
                ItemConfig item = _itemRegistry.GetItem(itemId);
                if (item != null)
                {
                    foreach (StatModifierData mod in item.Modifiers)
                    {
                        _modifierService.RegisterMultiplier(mod.StatType, mod.Multiplier);
                    }
                }
            }
        }
    }
}