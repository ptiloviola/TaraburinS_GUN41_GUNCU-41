using UnityEngine;
using Zenject;
using TpsShooter.Player;
using TpsShooter.Player.Configs;

[CreateAssetMenu(fileName = "GameSettingsInstaller", menuName = "Installers/GameSettingsInstaller")]
public class GameSettingsInstaller : ScriptableObjectInstaller
{
    public PlayerConfig PlayerConfig;

    public override void InstallBindings()
    {
        // Регистрируем инстанс конфига, чтобы Zenject мог его внедрять по [Inject]
        Container.BindInstance(PlayerConfig);
    }
}