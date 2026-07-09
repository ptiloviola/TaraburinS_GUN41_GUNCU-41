using UnityEngine;
using System.Collections.Generic;
using Enemy; // Подключаем неймспейс врага, чтобы видеть EnemyFacade
namespace Player
{
    public class PlayerVision
    {
        private readonly Transform _eyesTransform;
        private readonly PlayerConfig _config;
        
        // Храним врагов, которые СЕЙЧАС в зоне видимости
        private readonly HashSet<Collider> _visibleEnemies = new HashSet<Collider>();
        
        // Массив-буфер для физики (чтобы не выделять память каждый кадр)
        // 20 означает, что радар может засечь до 20 врагов одновременно
        private readonly Collider[] _hitColliders = new Collider[20]; 

        public PlayerVision(Transform eyesTransform, PlayerConfig config)
        {
            _eyesTransform = eyesTransform;
            _config = config;
        }

        public void Tick()
        {
            // 1. Ищем всех на слое врагов в радиусе sightRadius
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
                    
                    // Пытаемся получить Фасад и говорим ему "Проявись!"
                    if (col.TryGetComponent(out EnemyFacade enemy))
                    {
                        enemy.SetVisibility(true);
                    }
                }
            }

            // 2. Чистим список от тех, кто ушел из радиуса радара
            // RemoveWhere проходит по коллекции и удаляет то, что попадает под условие
            _visibleEnemies.RemoveWhere(col => 
            {
                // Если коллайдер из старого списка НЕ найден в текущем кадре
                if (!currentFrameEnemies.Contains(col))
                {
                    // Значит, он вышел из зоны. Говорим "Спрячься!"
                    if (col != null && col.TryGetComponent(out EnemyFacade enemy))
                    {
                        enemy.SetVisibility(false);
                    }
                    return true; // Команда для RemoveWhere: "Удаляй из списка"
                }
                return false; // Оставляем в списке
            });
        }
    }
}