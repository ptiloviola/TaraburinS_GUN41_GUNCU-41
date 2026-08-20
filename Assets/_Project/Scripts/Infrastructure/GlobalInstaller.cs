using UnityEngine;
using TpsShooter.Services.Input;
using TpsShooter.Services.Progress;
using TpsShooter.Services.SceneManagement;
using TpsShooter.Audio;
using Zenject;

namespace TpsShooter.Infrastructure
{
    public class GlobalInstaller : MonoInstaller
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioConfig _audioConfig;
        
        [SerializeField] private FootstepConfig _footstepConfig;

        public override void InstallBindings()
        {
            Container.BindInstance(_audioConfig).IfNotBound();
            
            Container.BindInstance(_footstepConfig).IfNotBound();

            Container.BindInterfacesAndSelfTo<GlobalAudioService>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<UnityInputService>().AsSingle().NonLazy();

            Container.Bind<GameProgressService>().AsSingle();
            Container.Bind<SceneLoaderService>().AsSingle();
        }
    }
}