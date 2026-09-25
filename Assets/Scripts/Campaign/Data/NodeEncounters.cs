using System;
using UnityEngine;
using Gameplay.Levels.Data;

namespace Gameplay.Campaign.Data
{
    [Serializable]
    public class CombatEncounter : INodeEncounter
    {
        [SerializeField] private LevelBlueprintConfig _config;
        public LevelBlueprintConfig Config => _config;

        public CombatEncounter(LevelBlueprintConfig config) => _config = config;

        public string DisplayName => _config != null ? _config.DisplayName : "Battle";
        public Sprite Icon => _config != null ? _config.MapIcon : null;
        public Color GlowColor => _config != null ? _config.MapGlowColor : Color.red;

        public void Accept(IEncounterVisitor visitor) => visitor.VisitCombat(_config);
    }

    [Serializable]
    public class ShopEncounter : INodeEncounter
    {
        [SerializeField] private ShopConfig _config;

        public ShopEncounter(ShopConfig config) => _config = config;

        public string DisplayName => "Shop";
        public Sprite Icon => _config != null ? _config.MapIcon : null;
        public Color GlowColor => _config != null ? _config.MapGlowColor : Color.yellow;

        public void Accept(IEncounterVisitor visitor) => visitor.VisitShop(_config);
    }

    [Serializable]
    public class EventEncounter : INodeEncounter
    {
        [SerializeField] private EventConfig _config;

        public EventEncounter(EventConfig config) => _config = config;

        public string DisplayName => _config != null ? _config.EventName : "Event";
        public Sprite Icon => _config != null ? _config.MapIcon : null;
        public Color GlowColor => _config != null ? _config.MapGlowColor : Color.cyan;

        public void Accept(IEncounterVisitor visitor) => visitor.VisitEvent(_config);
    }

    [Serializable]
    public class StartEncounter : INodeEncounter
    {
        [SerializeField] private Sprite _startIcon;
        
        public StartEncounter(Sprite startIcon) => _startIcon = startIcon;

        public string DisplayName => "Start";
        public Sprite Icon => _startIcon;
        public Color GlowColor => Color.white;

        public void Accept(IEncounterVisitor visitor) => visitor.VisitStart();
    }
}