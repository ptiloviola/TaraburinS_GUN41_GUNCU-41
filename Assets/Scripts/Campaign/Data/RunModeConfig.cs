using UnityEngine;
using Zenject;

namespace Gameplay.Campaign.Data
{
    public abstract class RunModeConfig : ScriptableObject
    {
        public abstract void InstallModeBindings(DiContainer container);
    }
}