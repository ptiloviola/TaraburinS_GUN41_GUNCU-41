using UnityEngine;
using Gameplay.Projectiles;
// НОВОЕ: Подключаем контракты и полезные нагрузки для создания фиктивной посылки
using Gameplay.Projectiles.Contracts;
using Gameplay.Projectiles.Payloads;
using Gameplay.Core;

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
        [SerializeField] private ModularProjectile _projectilePrefab;
        [SerializeField] private Transform _firePoint; 

        private void Start()
        {
            if (_visuals == null) _visuals = GetComponentInChildren<ProceduralTowerVisuals>();
            
            if (_logicalRotator == null)
            {
                _logicalRotator = transform.Find("Logical_Rotator");
            }
            if (_firePoint == null && _logicalRotator != null)
            {
                _firePoint = _logicalRotator.Find("FirePoint");
            }

            if (_visuals != null) _visuals.Initialize();
        }

        private void Update()
        {
            if (_visuals == null) return;

            if (_logicalRotator != null)
            {
                _logicalRotator.rotation = Quaternion.Euler(0, _rotationValue, 0);
            }
            else
            {
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
                
                Vector3 fakeTargetPos = _logicalRotator != null ? 
                    _logicalRotator.position + _logicalRotator.forward * 5f : 
                    transform.position + Vector3.forward * 5f;
                    
                _visuals.PlayShootAnimation(fakeTargetPos);

                if (_projectilePrefab != null)
                {
                    Transform spawnPoint = _firePoint != null ? _firePoint : _logicalRotator;
                    
                    GameObject fakeTargetObj = new GameObject("FakeTarget_Test");
                    fakeTargetObj.transform.position = fakeTargetPos;
                    Destroy(fakeTargetObj, 2f); 

                    var projectile = Instantiate(_projectilePrefab, spawnPoint.position, spawnPoint.rotation);
                    
                    // ИСПРАВЛЕНО: Создаем фиктивную посылку с нулевым уроном для теста
                    IProjectilePayload testPayload = new SingleTargetPayload(new DamagePayload(0f, DamageType.Physical));
                    
                    // Запускаем ядро лететь в невидимую цель, передавая правильный интерфейс
                    projectile.Launch(fakeTargetObj.transform, testPayload, null);
                }
            }
        }
    }
}