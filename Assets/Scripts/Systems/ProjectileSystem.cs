using Netologia.Behaviours;
using Netologia.TowerDefence;
using Netologia.TowerDefence.Behaviors;
using UnityEngine;
using Zenject;

namespace Netologia.Systems
{
	public class ProjectileSystem : GameObjectPoolContainer<Projectile>, Director.IManualUpdate
	{
		private EffectSystem _effects;		//injected
		
		[SerializeField, Min(0.01f)]
		private float _hitDistance = 0.3f;
		
		public void ManualUpdate()
		{
			var dt = TimeManager.DeltaTime;
			foreach (var pool in this)
			{
				foreach (var projectile in pool)
				{
					// 2.1. Смещаем снаряд в сторону цели
					projectile.transform.position = Vector3.MoveTowards(
						projectile.transform.position,
						projectile.TargetPosition,
						projectile.MoveSpeed * dt);

					// 2.2. Если цель в зоне поражения:
					var sqrDistance = Vector3.SqrMagnitude(projectile.transform.position - projectile.TargetPosition);
					if (sqrDistance <= _hitDistance)
					{
						Debug.Log($"[ProjectileSystem] Projectile {projectile.name} (id={projectile.ID}) HIT target at {projectile.transform.position}! sqrDist={sqrDistance:F4} <= hitDist={_hitDistance:F4}");
						// 2.2.1. Создаем хит эффект
						if (projectile.HasEffect)
						{
							var effect = _effects[projectile.HitEffect].Get;
							effect.transform.position = projectile.transform.position;
							effect.Play();
						}

						// 2.2.2. Отыгрываем звук попадания
						if (projectile.HasSound)
						{
							AudioManager.PlayHit(projectile.HitSound);
						}

						// 2.2.4. Наносим урон цели
						projectile.DealDamage();

						// 2.2.3. Убираем снаряд
						pool.ReturnElement(projectile.ID);
					}
				}
			}
		}

		public void OnDespawnUnit(int unitID)
		{
			foreach (var pool in this)
				foreach (var projectile in pool)
					if(projectile.TargetID == unitID)
						projectile.ResetTarget();
		}

		[Inject]
		private void Construct(EffectSystem effects)
		{
			(_effects) = (effects);
			//SqrtMagnitude optimization
			_hitDistance *= _hitDistance;
		}
	}
}