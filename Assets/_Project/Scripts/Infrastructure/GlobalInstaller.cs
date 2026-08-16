using UnityEngine;
using TpsShooter.Services.Input;
using TpsShooter.Services.Progress;
using TpsShooter.Services.SceneManagement;
using TpsShooter.Audio; // <-- Добавлено
using Zenject;

namespace TpsShooter.Infrastructure
{
    public class GlobalInstaller : MonoInstaller
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioConfig _audioConfig;
        
        // <--- ДОБАВЛЯЕМ КОНФИГ ШАГОВ СЮДА --->
        [SerializeField] private FootstepConfig _footstepConfig;

        public override void InstallBindings()
        {
            // Биндим сам конфиг, чтобы GlobalAudioService мог получить его в конструктор
            Container.BindInstance(_audioConfig).IfNotBound();
            
            // <--- БИНДИМ КОНФИГ ШАГОВ ГЛОБАЛЬНО --->
            Container.BindInstance(_footstepConfig).IfNotBound();

            // Биндим интерфейс звука к его реализации (AsSingle = один на всю игру)
            Container.BindInterfacesAndSelfTo<GlobalAudioService>().AsSingle().NonLazy();

            // Ввод
            Container.BindInterfacesAndSelfTo<UnityInputService>().AsSingle();

            // Прогресс и Сцены
            Container.Bind<GameProgressService>().AsSingle();
            Container.Bind<SceneLoaderService>().AsSingle();
        }
    }
}