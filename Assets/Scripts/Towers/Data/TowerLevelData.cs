using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Towers.Data.Modules;
using Gameplay.Core.Attributes; 

namespace Gameplay.Towers.Data
{
    [System.Serializable]
    public class TowerLevelData
    {
        public int UpgradeCost;
        public GameObject VisualPrefab;

        [Header("Модули поведения")]
        [SerializeReference, SubclassSelector]
        public List<IModuleDescriptor> Modules = new List<IModuleDescriptor>();

        public IEnumerable<IModuleDescriptor> GetActiveModules()
        {
            return Modules;
        }

        // Удобный метод для контроллеров, чтобы они могли сами достать свой модуль
        public T GetModule<T>() where T : class, IModuleDescriptor
        {
            return Modules.FirstOrDefault(m => m is T) as T;
        }
    }
}