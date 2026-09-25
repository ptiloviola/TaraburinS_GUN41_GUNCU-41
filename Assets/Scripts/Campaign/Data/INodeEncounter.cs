using UnityEngine;
using Gameplay.Levels.Data;

namespace Gameplay.Campaign.Data
{
    public interface IEncounterVisitor
    {
        void VisitCombat(LevelBlueprintConfig config);
        void VisitShop(ShopConfig config);
        void VisitEvent(EventConfig config);
        void VisitStart();
    }


    public interface INodeEncounter
    {
        string DisplayName { get; }
        Sprite Icon { get; }
        Color GlowColor { get; }
        

        void Accept(IEncounterVisitor visitor);
    }
}