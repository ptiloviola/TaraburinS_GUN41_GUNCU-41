using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Towers.Data.Modules;
using Gameplay.Combat.Attributes; 

namespace Gameplay.Towers.Data
{
    [System.Serializable]
    public class TowerLevelData
    {
        public int UpgradeCost;
        
        [Tooltip("Полноценный префаб башни со всеми скриптами, коллайдерами и логикой")]
        public GameObject TowerPrefab; 

        [Header("Модули поведения")]
        [SerializeReference, SubclassSelector]
        public List<IModuleDescriptor> Modules = new List<IModuleDescriptor>();

        public IEnumerable<IModuleDescriptor> GetActiveModules()
        {
            return Modules;
        }

        public T GetModule<T>() where T : class, IModuleDescriptor
        {
            return Modules.FirstOrDefault(m => m is T) as T;
        }
    }
}