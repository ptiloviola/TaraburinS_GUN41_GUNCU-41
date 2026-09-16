using System.Collections.Generic;
using UnityEngine;
using Gameplay.Projectiles.Contracts;
using Gameplay.Auras.Data;
using Gameplay.Auras;
using Gameplay.Core.Statuses.Data;
using Gameplay.Core;
using Gameplay.Core.Attributes;
using Gameplay.Auras.Factories;

namespace Gameplay.Projectiles.Data
{
    [CreateAssetMenu(fileName = "NewSpawnAuraPayload", menuName = "TD/Projectiles/Payloads/Spawn Aura")]
    public class SpawnAuraPayloadConfig : PayloadConfig
    {
        [Header("Префаб Ауры")]
        public LingeringAuraFacade AuraPrefab;
        public LayerMask EnemyMask;

        [Header("Настройки Ауры")]
        public float Radius = 2f;
        public float Duration = 3f;
        public float TickRate = 0.5f;

        [Header("Статусы (Комбинируемые)")]
        [SerializeReference, SubclassSelector]
        public List<IStatusConfig> StatusEffects = new List<IStatusConfig>();

        public override IProjectilePayload CreatePayload(DamagePayload payload)
        {
            AuraSetup setup = new AuraSetup(Radius, Duration, TickRate, StatusEffects);
            return new SpawnAuraPayload(setup, AuraPrefab, EnemyMask);
        }
    }

    public class SpawnAuraPayload : IProjectilePayload, IRequireAuraFactory
    {
        private readonly AuraSetup _setup;
        private readonly LingeringAuraFacade _prefab;
        private readonly LayerMask _enemyMask;
        
        private AuraZoneFactory _factory;

        public SpawnAuraPayload(AuraSetup setup, LingeringAuraFacade prefab, LayerMask enemyMask)
        {
            _setup = setup;
            _prefab = prefab;
            _enemyMask = enemyMask;
        }

        public void SetFactory(AuraZoneFactory factory)
        {
            _factory = factory;
            Debug.Log("<color=yellow>[Payload] Фабрика успешно внедрена в снаряд!</color>");
        }

        public void Apply(Transform target, Vector3 hitPoint)
        {
            Debug.Log($"<color=orange>[Payload] Попытка спавна лужи в координатах: {hitPoint}</color>");
            
            if (_factory == null)
            {
                Debug.LogError("[Payload] ОШИБКА: AuraZoneFactory = null. Внедрение не сработало!");
                return;
            }
            if (_prefab == null)
            {
                Debug.LogError("[Payload] ОШИБКА: AuraPrefab = null. Проверь SpawnAuraPayloadConfig!");
                return;
            }

            var pool = _factory.GetPool(_prefab);
            pool.Spawn(_setup, hitPoint, _enemyMask);
            
            Debug.Log("<color=green>[Payload] Лужа успешно заспавнена из пула!</color>");
        }
    }
}