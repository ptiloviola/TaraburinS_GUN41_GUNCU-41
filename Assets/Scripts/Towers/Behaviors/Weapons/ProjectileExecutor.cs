
using UnityEngine;
using Gameplay.Towers.Behaviors.Weapons;
using Zenject;

public class ProjectileExecutor : MonoBehaviour, IAttackExecutor
{
    private KinematicProjectile.Pool _projectilePool;
    [Inject]
    public void Construct(KinematicProjectile.Pool projectilePool)
    {
        _projectilePool = projectilePool;
    }   


    public void ExecuteAttack(Transform target, float damage, Transform firePoint)
    {
        if (_projectilePool == null)
        {
            Debug.LogError("[ProjectileExecutor] Пул снарядов не установлен!");
            return;
        }

        // 1. Получаем ядро из пула (или создаем новое, если пул пуст)

        var projectile = _projectilePool.Spawn(); 
        // 2. Ставим его в дуло пушки
        projectile.transform.position = firePoint.position;
        projectile.transform.rotation = firePoint.rotation;

        // 3. Передаем ему цель и приказ лететь
        projectile.Launch(target, damage, _projectilePool);

    }

}
