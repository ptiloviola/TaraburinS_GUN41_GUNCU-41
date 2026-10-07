using UnityEngine;
using Zenject;

namespace Gameplay.Enemies.Data.Death
{
    public abstract class DeathBehaviorConfig : ScriptableObject
    {
        public abstract void Execute(Transform enemyTransform, SignalBus signalBus);
    }
}