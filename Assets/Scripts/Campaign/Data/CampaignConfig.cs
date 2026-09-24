using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Gameplay.Campaign.Services;

namespace Gameplay.Campaign.Data
{
    [CreateAssetMenu(fileName = "NewCampaignConfig", menuName = "TD/Campaign/Linear Config")]
    public class CampaignConfig : RunModeConfig
    {
        [Header("Карта кампании (Узлы)")]
        [SerializeField] private List<MapNode> _nodes = new List<MapNode>();

        public IReadOnlyList<MapNode> Nodes => _nodes;

        public override void InstallModeBindings(DiContainer container)
        {
            container.BindInstance(this).AsSingle();
            container.Bind<IRunDirectorService>().To<LinearRunDirector>().AsSingle();
        }
    }
}