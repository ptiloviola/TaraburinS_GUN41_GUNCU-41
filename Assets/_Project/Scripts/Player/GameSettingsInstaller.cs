using UnityEngine;
using Zenject;
using TpsShooter.Player.Configs;

[CreateAssetMenu(fileName = "GameSettingsInstaller", menuName = "Installers/GameSettingsInstaller")]
public class GameSettingsInstaller : ScriptableObjectInstaller
{
    public PlayerConfig PlayerConfig;

    public override void InstallBindings()
    {
        Container.BindInstance(PlayerConfig);
    }
}