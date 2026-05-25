using UnityEngine;

namespace Gameplay.Towers.Visuals
{
    public class TowerVisualsTester : MonoBehaviour
    {
        [Header("Ссылка на визуал")]
        [SerializeField] private ProceduralTowerVisuals _visuals;

        [Header("Ползунок вращения")]
        [SerializeField] private Transform _logicalRotator;
        [Range(-180f, 180f)]
        [SerializeField] private float _rotationValue;

        [Header("Кнопки теста (Галочки)")]
        [SerializeField] private bool _triggerBuild; // Галочка для теста появления

        // Добавь эту переменную в самый верх тестера к остальным флажкам:
        [SerializeField] private bool _triggerShoot;

        private void Update()
        {
            if (_visuals != null)
            {
                // Каждый кадр передаем значение ползунка в визуальный скрипт
                _visuals.SetRotation(_rotationValue);
            }
            // Если в инспекторе нажали галочку Trigger Build
            if (_triggerBuild && _visuals != null)
            {
                _triggerBuild = false; // Сразу выключаем галочку обратно
                _visuals.PlayBuildAnimation(); // Запускаем анимацию
            }
            // А этот кусок добавь в конец метода Update():
            if (_triggerShoot && _visuals != null)
            {
                _triggerShoot = false;
                _visuals.PlayShootAnimation();
            }
        }
    }
}