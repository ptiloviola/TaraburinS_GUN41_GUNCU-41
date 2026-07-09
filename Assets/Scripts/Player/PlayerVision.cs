using UnityEngine;
using System.Collections.Generic;
using Infrastructure.Interfaces; // ДОБАВЛЕНО: Подключаем наши интерфейсы-контракты
using Player.Config;

namespace Player
{
    public class PlayerVision
    {
        private readonly Transform _eyesTransform;
        private readonly RadarConfig _config;
        
        // Храним коллайдеры, которые СЕЙЧАС в зоне видимости
        private readonly HashSet<Collider> _visibleEnemies = new HashSet<Collider>();
        
        // Массив-буфер для физики (чтобы не выделять память каждый кадр)
        private readonly Collider[] _hitColliders = new Collider[20]; 

        public PlayerVision(Transform eyesTransform, RadarConfig config)
        {
            _eyesTransform = eyesTransform;
            _config = config;
        }

        public void Tick()
        {
            // 1. Ищем всех на слое врагов (EnemyRadar) в радиусе sightRadius
            int count = Physics.OverlapSphereNonAlloc(
                _eyesTransform.position, 
                _config.sightRadius, 
                _hitColliders, 
                _config.enemyLayer
            );

            // Множество тех, кого мы поймали ИМЕННО В ЭТОМ КАДРЕ
            HashSet<Collider> currentFrameEnemies = new HashSet<Collider>();

            for (int i = 0; i < count; i++)
            {
                Collider col = _hitColliders[i];
                currentFrameEnemies.Add(col);

                // Если этого коллайдера нет в нашем списке видимых - значит он только что вошел в зону
                if (!_visibleEnemies.Contains(col))
                {
                    _visibleEnemies.Add(col);
                    
                    // ИЗМЕНЕНИЕ ЗДЕСЬ: Ищем любой объект, реализующий интерфейс IVisibleTarget
                    IVisibleTarget visibleTarget = col.GetComponentInParent<IVisibleTarget>();
                    if (visibleTarget != null)
                    {
                        visibleTarget.SetVisibility(true);
                    }
                }
            }

            // 2. Чистим список от тех, кто ушел из радиуса радара
            _visibleEnemies.RemoveWhere(col => 
            {
                // Если коллайдер из старого списка НЕ найден в текущем кадре
                if (!currentFrameEnemies.Contains(col))
                {
                    // Значит, он вышел из зоны. Говорим "Спрячься!"
                    if (col != null)
                    {
                        // ИЗМЕНЕНИЕ ЗДЕСЬ: Снова обращаемся через интерфейс
                        IVisibleTarget visibleTarget = col.GetComponentInParent<IVisibleTarget>();
                        if (visibleTarget != null)
                        {
                            visibleTarget.SetVisibility(false);
                        }
                    }
                    return true; // Команда для RemoveWhere: "Удаляй из списка"
                }
                return false; // Оставляем в списке
            });
        }
    }
}