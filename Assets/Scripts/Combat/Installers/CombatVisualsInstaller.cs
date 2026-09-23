using UnityEngine;
using Zenject;
using Gameplay.Enemies.Visuals;

namespace Gameplay.Combat.Installers
{
    public class CombatVisualsInstaller : MonoInstaller
    {
        [Header("Настройки и Префабы")]
        [SerializeField] private DamageVisualSettings _damageSettings;
        [SerializeField] private FloatingText _floatingTextPrefab;

        public override void InstallBindings()
        {

            Container.DeclareSignal<DamageReceivedSignal>();

            Container.BindInstance(_damageSettings).AsSingle();

            Container.BindMemoryPool<FloatingText, FloatingText.Pool>()
                .WithInitialSize(30) 
                .FromComponentInNewPrefab(_floatingTextPrefab)
                .UnderTransformGroup("Pool_FloatingTexts"); 

            Container.BindInterfacesTo<GlobalDamageVisualizer>().AsSingle();
            
            Debug.Log("<color=green>[Zenject] CombatVisualsInstaller: Глобальная шина урона активна.</color>");
        }
    }
}