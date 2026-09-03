using UnityEngine;
using Zenject;
using Gameplay.Enemies.Visuals;

namespace Gameplay.Core.Installers
{
    public class CombatVisualsInstaller : MonoInstaller
    {
        [Header("Пулы UI и Текста")]
        [SerializeField] private FloatingText _floatingTextPrefab;

        // В будущем здесь появятся:
        // [SerializeField] private GameObject _explosionPrefab;
        // [SerializeField] private GameObject _bloodSplatterPrefab;

        public override void InstallBindings()
        {
            BindFloatingTextPool();
            
            Debug.Log("<color=green>[Zenject] CombatVisualsInstaller: Боевые визуалы зарегистрированы.</color>");
        }

        private void BindFloatingTextPool()
        {
            Container.BindMemoryPool<FloatingText, FloatingText.Pool>()
                .WithInitialSize(30) 
                .FromComponentInNewPrefab(_floatingTextPrefab)
                .UnderTransformGroup("Pool_FloatingTexts"); 
        }
    }
}