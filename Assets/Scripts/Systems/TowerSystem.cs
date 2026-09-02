using Netologia.Behaviours;
using Netologia.TowerDefence;
using Netologia.TowerDefence.Behaviors;
using UnityEngine;
using Zenject;

namespace Netologia.Systems
{
	public class TowerSystem : GameObjectPoolContainer<Tower>, Director.IManualUpdate
	{
		private UnitSystem _units;				//injected
		private ProjectileSystem _projectiles;	//injected

		private float _logCooldown;

		public void ManualUpdate()
		{
			var dt = TimeManager.DeltaTime;
			_logCooldown -= dt;
			var shouldLogStatus = _logCooldown <= 0f;
			if (shouldLogStatus)
			{
				_logCooldown = 1.5f;
				Debug.Log($"[TowerSystem] Tick. Pools: {_pools.Count}, Active towers: {CountActive}");
			}

			foreach (var pool in this)
			{
				foreach (var tower in pool)
				{
					if (shouldLogStatus)
					{
						Debug.Log($"[TowerSystem] Tower {tower.name} (lvl {tower.Level}, pos {tower.transform.position}, range {tower.Range}, reloading={tower.IsReloading}, hasTarget={tower.HasTarget})");
					}

					// 1.1. Если башня в перезарядке:
					if (tower.IsReloading)
					{
						// 1.1.1. Перезаряжаем
						tower.DecrementAttackReload(dt);
						// 1.1.2. Если башня все равно в КД - ВЫХОД
						if (tower.IsReloading)
							continue;
						Debug.Log($"[TowerSystem] Tower {tower.name} finished reload!");
					}

					// 1.2. Проверяем валидность текущего таргета (если есть):
					// если цель уничтожена, неактивна или вышла из радиуса атаки - сбрасываем таргет
					if (tower.HasTarget)
					{
						if (tower.Target == null || !tower.Target.gameObject.activeInHierarchy || tower.Target.CurrentHealth <= 0)
						{
							Debug.Log($"[TowerSystem] Tower {tower.name} target died or inactive. Resetting target.");
							tower.Target = null;
						}
						else
						{
							var sqrDistance = Vector3.SqrMagnitude(tower.Target.transform.position - tower.transform.position);
							if (sqrDistance > tower.Range * tower.Range)
							{
								Debug.Log($"[TowerSystem] Tower {tower.name} target left range (sqrDist={sqrDistance:F2} > sqrRange={tower.Range * tower.Range:F2}). Resetting target.");
								tower.Target = null;
							}
						}
					}

					// 1.2. Если у башни нет таргета:
					if (!tower.HasTarget)
					{
						// 1.2.1. Ищем таргет в зоне атаки ближайшего
						var target = _units.FindTarget(tower.transform.position, tower.Range);
						// 1.2.2. Если ближайшего нет - ВЫХОД
						if (target == null)
						{
							if (shouldLogStatus)
								Debug.Log($"[TowerSystem] Tower {tower.name} FindTarget returned null in range {tower.Range}");
							continue;
						}
						// 1.2.3. Если ближайший есть - задаем таргет
						tower.Target = target;
						Debug.Log($"[TowerSystem] Tower {tower.name} acquired target: {target.name} (id={target.ID}, pos={target.transform.position})");
					}

					// 1.3. Стреляем:
					Debug.Log($"[TowerSystem] Tower {tower.name} SHOOTING at {tower.Target?.name}! ProjectilePrefab={tower.Projectile?.name}");
					// 1.3.1. Создаем снаряд и указываем цель
					var projectile = _projectiles[tower.Projectile].Get;
					Debug.Log($"[TowerSystem] Projectile instance received: {projectile?.name}, active={projectile?.gameObject.activeSelf}, id={projectile?.ID}");
					projectile.PrepareData(tower.transform.position, tower.Target, tower.Damage, tower.AttackElemental);

					// 1.3.2. Запускаем КД
					// 1.3.3. Показываем эффект выстрела
					// 1.3.4. Отыгрываем звук выстрела
					tower.Attack();
					Debug.Log($"[TowerSystem] tower.Attack() called! Tower is now reloading={tower.IsReloading}");
				}
			}
		}

		public void OnDespawnUnit(int unitID)
		{
			foreach (var pair in this)
				foreach (var tower in pair)
					if (tower.TargetID == unitID)
						tower.Target = null;
		}
		
		[Inject]
		private void Construct(UnitSystem units, ProjectileSystem projectiles)
		{
			_units = units;
			_projectiles = projectiles;
			Debug.Log($"[TowerSystem] Construct injected: units={units != null}, projectiles={projectiles != null}");
		}
	}
}