using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Units.Data
{
    [CreateAssetMenu(fileName = "DefenderRegistry", menuName = "TD/Registries/Defender Registry")]
    public class DefenderRegistry : ScriptableObject
    {
        public List<DefenderConfig> Defenders;
    }
}