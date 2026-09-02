using System;
using Behaviours;
using JetBrains.Annotations;
using Netologia.Behaviours;
using Netologia.TowerDefence;
using Netologia.TowerDefence.Behaviors;
using Netologia.TowerDefence.Settings;
using UnityEngine;
using Zenject;

namespace Netologia.Systems
{
	public class UnitSystem : GameObjectPoolContainer<Unit>, Director.IManualUpdate
	{
		private Director _director;					//injected
		private EffectSystem _effects;				//injected
		private Constants _constants;				//injected
		private Vector3[] _path;					//injected

		[SerializeField, Min(0.01f)]
		private float _arrivalDistance = 0.1f;

		public event Action<int> OnDespawnUnitHandler;

		[CanBeNull]
		public Unit FindTarget(in Vector3 position, float range)
		{
			var sqrRange = range * range;
			var target = default(Unit);
			var checkedCount = 0;
			foreach (var pair in this)
			{
				foreach (var unit in pair)
				{
					checkedCount++;
					if (unit.CurrentHealth <= 0) continue;
					var distance = Vector3.SqrMagnitude(unit.transform.position - position);
					if (distance < sqrRange)
						(sqrRange, target) = (distance, unit);
				}
			}

			if (target != null)
			{
				Debug.Log($"[UnitSystem.FindTarget] SUCCESS! Checked {checkedCount} units. Target: {target.name} (id={target.ID}, hp={target.CurrentHealth}, pos={target.transform.position}), distance={Mathf.Sqrt(sqrRange):F2}, maxRange={range}");
			}

			return target;
		}

		public void ManualUpdate()
		{
			var dt = TimeManager.DeltaTime;
			var time = TimeManager.Time;

			foreach (var pool in this)
			{
				foreach (var unit in pool)
				{
					// 3.1 & 3.2. Если ХП нет (убит снарядом):
					if (unit.CurrentHealth <= 0)
					{
						DespawnUnit(unit, unit.transform.position);
						continue;
					}

					// Снятие истёкших эффектов дебаффов
					unit.TryRemoveEffect(time, ElementalType.Fire);
					unit.TryRemoveEffect(time, ElementalType.Ice);

					// Урон от эффекта горения (DoT)
					var fireCount = unit.CountEffect(ElementalType.Fire);
					if (fireCount > 0)
					{
						unit.CurrentHealth -= unit.Stats.Health * _constants.FireDebuffDamageMult * fireCount * dt;
						if (unit.CurrentHealth <= 0)
						{
							DespawnUnit(unit, unit.transform.position);
							continue;
						}
					}

					// Обновление анимации
					unit.Visual.ManualUpdate(dt);

					// 3.3. Если ХП есть: Смещаемся к выходу
					if (_path != null && unit.PathIndex < _path.Length)
					{
						unit.transform.position = Vector3.MoveTowards(
							unit.transform.position,
							_path[unit.PathIndex],
							unit.MoveSpeed * dt);

						// Проверка достижения текущей путевой точки
						var sqrDist = Vector3.SqrMagnitude(unit.transform.position - _path[unit.PathIndex]);
						if (sqrDist <= _arrivalDistance)
						{
							unit.PathIndex++;

							// 3.4. Если выход достигнут:
							if (unit.PathIndex >= _path.Length)
							{
								// 3.4.1. Убираем врага
								OnDespawnUnitHandler?.Invoke(unit.ID);
								pool.ReturnElement(unit.ID);

								// 3.4.2. Наносим урон игроку
								_director.AddPlayerDamage(1);
							}
						}
					}
				}
			}
		}

		private void DespawnUnit(Unit unit, in Vector3 position)
		{
			OnDespawnUnitHandler?.Invoke(unit.ID);

			//Create HitEffect
			if (unit.HasEffect)
			{
				var effect = _effects[unit.DieEffect].Get;
				effect.transform.position = position;
				effect.Play();
			}
			//Play sound
			if (unit.HasSound)
			{
				AudioManager.PlayHit(unit.DieSound);
			}

			var director = _director ?? Director.Instance;
			var reward = unit.Stats.Cost > 0 ? unit.Stats.Cost : 5;
			if (director != null)
			{
				director.AddMoney(reward);
				Debug.Log($"[UnitSystem] DespawnUnit (KILL): Unit {unit.name} (id={unit.ID}) DIED! Rewarded {reward} gold.");
			}
			else
			{
				Debug.LogError($"[UnitSystem] DespawnUnit: Director is null! Cannot add money for {unit.name}");
			}

			this[unit.Ref].ReturnElement(unit.ID);
		}
		
		[Inject]
		private void Construct(EffectSystem effects, Director director, Constants constants, WaveController path)
		{
			(_effects, _director, _constants, _path) = (effects, director, constants, path.GetPath());
			_arrivalDistance *= _arrivalDistance;
			AwakeMethod = t => t.Constants = _constants;
			Debug.Log($"[UnitSystem] Construct injected: director={director != null}, pathCount={_path?.Length}");
		}
	}
}