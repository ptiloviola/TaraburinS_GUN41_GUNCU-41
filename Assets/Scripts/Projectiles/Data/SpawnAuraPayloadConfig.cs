using System.Collections.Generic;
using UnityEngine;
using Gameplay.Projectiles.Contracts;
using Gameplay.Auras.Data;
using Gameplay.Auras;
using Gameplay.Combat.Statuses.Data;
using Gameplay.Combat;
using Gameplay.Core.Attributes;
using Gameplay.Auras.Factories;

namespace Gameplay.Projectiles.Data
{
    [CreateAssetMenu(fileName = "SpawnAuraPayload", menuName = "TD/Projectiles/Payloads/Spawn Aura")]
    public class SpawnAuraPayloadConfig : PayloadConfig
    {
        [Header("Префаб Ауры")]
        public LingeringAuraFacade AuraPrefab;
        public LayerMask EnemyMask;
        
        [Header("Ограничения целей (Аура)")]
        public TargetType AllowedTargets = TargetType.Ground | TargetType.Air;

        [Header("Настройки Ауры")]
        public float Radius = 2f;
        public float Duration = 3f;
        public float TickRate = 0.5f;

        [Header("Статусы (Комбинируемые)")]
        [SerializeReference, SubclassSelector]
        public List<IStatusConfig> StatusEffects = new List<IStatusConfig>();

        public override IProjectilePayload CreatePayload(DamagePayload payload)
        {
            AuraSetup setup = new AuraSetup(Radius, Duration, TickRate, StatusEffects, payload);
            return new SpawnAuraPayload(setup, AuraPrefab, EnemyMask, AllowedTargets); 
        }
    }

    public class SpawnAuraPayload : IProjectilePayload, IRequireAuraFactory
    {
        private readonly AuraSetup _setup;
        private readonly LingeringAuraFacade _prefab;
        private readonly LayerMask _enemyMask;
        private readonly TargetType _allowedTargets;
        
        private AuraZoneFactory _factory;

        public SpawnAuraPayload(AuraSetup setup, LingeringAuraFacade prefab, LayerMask enemyMask, TargetType allowedTargets)
        {
            _setup = setup;
            _prefab = prefab;
            _enemyMask = enemyMask;
            _allowedTargets = allowedTargets;
        }

        public void SetFactory(AuraZoneFactory factory)
        {
            _factory = factory;
            Gameplay.Tools.GameLogger.Log("<color=yellow>[Payload] Фабрика успешно внедрена в снаряд!</color>");
        }

        public void Apply(Transform target, Vector3 hitPoint)
        {
            Gameplay.Tools.GameLogger.Log($"<color=orange>[Payload] Попытка спавна лужи в координатах: {hitPoint}</color>");
            
            if (_factory == null)
            {
                Gameplay.Tools.GameLogger.LogError("[Payload] ОШИБКА: AuraZoneFactory = null. Внедрение не сработало!");
                return;
            }
            if (_prefab == null)
            {
                Gameplay.Tools.GameLogger.LogError("[Payload] ОШИБКА: AuraPrefab = null. Проверь SpawnAuraPayloadConfig!");
                return;
            }

            var pool = _factory.GetPool(_prefab);
            
            pool.Spawn(_setup, hitPoint, _enemyMask, _allowedTargets);
            
            Gameplay.Tools.GameLogger.Log("<color=green>[Payload] Лужа успешно заспавнена из пула!</color>");
        }
    }
}