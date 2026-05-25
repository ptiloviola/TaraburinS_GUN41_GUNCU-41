using UnityEngine;

namespace Gameplay.Towers.Visuals
{
    public class TowerVisualsTester : MonoBehaviour
    {
        [Header("Ссылка на визуал")]
        [SerializeField] private ProceduralTowerVisuals _visuals;

        [Header("Ссылка на логический ротатор (для симуляции)")]
        [SerializeField] private Transform _logicalRotator;

        [Header("Ползунок вращения")]
        [Range(-180f, 180f)]
        [SerializeField] private float _rotationValue;

        [Header("Кнопки теста (Галочки)")]
        [SerializeField] private bool _triggerBuild;
        [SerializeField] private bool _triggerShoot;

        private void Start()
        {
            if (_visuals == null) _visuals = GetComponentInChildren<ProceduralTowerVisuals>();
            
            // АВТОПОИСК РОТАТОРА ДЛЯ ТЕСТЕРА:
            // Ищем его на том же уровне, где и в боевом коде
            if (_logicalRotator == null)
            {
                _logicalRotator = transform.Find("Logical_Rotator");
            }

            if (_visuals != null) _visuals.Initialize();
        }

        private void Update()
        {
            if (_visuals == null) return;

            // ИСТИННАЯ СИМУЛЯЦИЯ:
            // Если ротатор в префабе есть, мы крутим ЕГО (имитируем работу AttackBehavior).
            // Визуалка в LateUpdate сама считает этот поворот, добавит офсет и развернет Turret!
            if (_logicalRotator != null)
            {
                _logicalRotator.rotation = Quaternion.Euler(0, _rotationValue, 0);
            }
            else
            {
                // Резервный ручной режим для "голых" визуальных моделей без логического слоя
                _visuals.SetRotation(_rotationValue);
            }

            if (_triggerBuild)
            {
                _triggerBuild = false;
                _visuals.PlayBuildAnimation();
            }

            if (_triggerShoot)
            {
                _triggerShoot = false;
                _visuals.PlayShootAnimation();
            }
        }
    }
}