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

		public void ManualUpdate()
		{
			var dt = TimeManager.DeltaTime;
			foreach (var pool in this)
			{
				foreach (var tower in pool)
				{
					// 1.1. Если башня в перезарядке:
					// 1.1.1. Перезаряжаем
					// 1.1.2. Если башня все равно в КД - ВЫХОД
					if (!tower.DecrementAttackReload(dt))
						continue;

					// 1.2. Если у башни есть таргет, но он вышел из радиуса атаки - сбрасываем таргет
					if (tower.HasTarget)
					{
						var sqrDistance = Vector3.SqrMagnitude(tower.Target.transform.position - tower.transform.position);
						if (sqrDistance > tower.Range * tower.Range)
							tower.Target = null;
					}

					// 1.2. Если у башни нет таргета:
					if (!tower.HasTarget)
					{
						// 1.2.1. Ищем таргет в зоне атаки ближайшего
						var target = _units.FindTarget(tower.transform.position, tower.Range);
						// 1.2.2. Если ближайшего нет - ВЫХОД
						if (target == null)
							continue;
						// 1.2.3. Если ближайший есть - задаем таргет
						tower.Target = target;
					}

					// 1.3. Стреляем:
					// 1.3.1. Создаем снаряд и указываем цель
					var projectile = _projectiles[tower.Projectile].Get;
					projectile.PrepareData(tower.transform.position, tower.Target, tower.Damage, tower.AttackElemental);

					// 1.3.2. Запускаем КД
					// 1.3.3. Показываем эффект выстрела
					// 1.3.4. Отыгрываем звук выстрела
					tower.Attack();
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
		}
	}
}