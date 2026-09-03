using UnityEngine;
using Zenject;
using Gameplay.Enemies.Visuals;
using Gameplay.Core;

namespace Gameplay.Core.Installers
{
    public class CombatVisualsInstaller : MonoInstaller
    {
        [Header("Настройки и Префабы")]
        [SerializeField] private DamageVisualSettings _damageSettings;
        [SerializeField] private FloatingText _floatingTextPrefab;

        public override void InstallBindings()
        {

            // 1. Объявляем наш сигнал (Шина уже существует в другом инсталлере)
            Container.DeclareSignal<DamageReceivedSignal>();

            // 2. Биндим настройки
            Container.BindInstance(_damageSettings).AsSingle();

            // 3. Биндим пул текстов
            Container.BindMemoryPool<FloatingText, FloatingText.Pool>()
                .WithInitialSize(30) 
                .FromComponentInNewPrefab(_floatingTextPrefab)
                .UnderTransformGroup("Pool_FloatingTexts"); 

            // 4. Биндим нашего менеджера
            Container.BindInterfacesTo<GlobalDamageVisualizer>().AsSingle();
            
            Debug.Log("<color=green>[Zenject] CombatVisualsInstaller: Глобальная шина урона активна.</color>");
        }
    }
}