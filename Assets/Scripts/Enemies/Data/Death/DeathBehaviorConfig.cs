using UnityEngine;

namespace Gameplay.Enemies.Data.Death
{
    public abstract class DeathBehaviorConfig : ScriptableObject
    {
        public abstract void Execute(EnemyFacade facade);
    }
}