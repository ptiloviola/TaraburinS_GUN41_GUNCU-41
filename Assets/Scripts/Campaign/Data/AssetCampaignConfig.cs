using UnityEngine;
using Zenject;
using Gameplay.Campaign.Services;

namespace Gameplay.Campaign.Data
{
    [CreateAssetMenu(fileName = "AssetCampaignConfig", menuName = "TD/Campaign/Asset Mode Config")]
    public class AssetCampaignConfig : RunModeConfig
    {
        [Tooltip("Ассет с графом карты, который мы создали в редакторе")]
        public CampaignGraphAsset GraphAsset;

        public override void InstallModeBindings(DiContainer container)
        {
            container.Bind<IRunDirectorService>()
                     .To<AssetBasedRunDirector>()
                     .AsSingle()
                     .WithArguments(GraphAsset);
        }
    }
}