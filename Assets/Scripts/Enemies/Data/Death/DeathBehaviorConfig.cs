using UnityEngine;

namespace Gameplay.Enemies.Data.Death
{
    public abstract class DeathBehaviorConfig : ScriptableObject
    {
        // Паттерн Команда/Стратегия: каждый конфиг смерти сам решает, что делать
        public abstract void Execute(EnemyFacade facade);
    }
}