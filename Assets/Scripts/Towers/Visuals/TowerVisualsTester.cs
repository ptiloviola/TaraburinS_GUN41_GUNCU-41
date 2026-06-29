using UnityEngine;
// Не забудь добавить этот using для доступа к снарядам:
using Gameplay.Projectiles;

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
        
        [Header("Тест Снарядов (Костыль без Zenject)")]
        [SerializeField] private KinematicProjectile _projectilePrefab;
        [SerializeField] private Transform _firePoint; // Откуда будет вылетать снаряд

        private void Start()
        {
            if (_visuals == null) _visuals = GetComponentInChildren<ProceduralTowerVisuals>();
            
            // АВТОПОИСК РОТАТОРА ДЛЯ ТЕСТЕРА:
            // Ищем его на том же уровне, где и в боевом коде
            if (_logicalRotator == null)
            {
                _logicalRotator = transform.Find("Logical_Rotator");
            }
            // Пытаемся автоматически найти FirePoint, если забыли назначить
            if (_firePoint == null && _logicalRotator != null)
            {
                _firePoint = _logicalRotator.Find("FirePoint");
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
                
                // 1. Вычисляем точку попадания (5 метров вперед по направлению дула)
                Vector3 fakeTargetPos = _logicalRotator != null ? 
                    _logicalRotator.position + _logicalRotator.forward * 5f : 
                    transform.position + Vector3.forward * 5f;
                    
                // 2. Запускаем визуал И передаем точку попадания (согласно интерфейсу)
                _visuals.PlayShootAnimation(fakeTargetPos);

                // 3. СПАВН СНАРЯДА (Если префаб назначен)
                if (_projectilePrefab != null)
                {
                    Transform spawnPoint = _firePoint != null ? _firePoint : _logicalRotator;
                    
                    // Создаем временную невидимую цель по тем же координатам fakeTargetPos
                    GameObject fakeTargetObj = new GameObject("FakeTarget_Test");
                    fakeTargetObj.transform.position = fakeTargetPos;
                    Destroy(fakeTargetObj, 2f); // Очищаем сцену через 2 секунды

                    // Создаем ядро (без пула)
                    var projectile = Instantiate(_projectilePrefab, spawnPoint.position, spawnPoint.rotation);
                    
                    // Запускаем ядро лететь в невидимую цель (передаем null вместо пула)
                    projectile.Launch(fakeTargetObj.transform, 0f, null);
                }
            }
        }
    }
}