using UnityEngine;
using System.Collections.Generic;
using Infrastructure.Interfaces;
using Player.Config;

namespace Player
{
    public class PlayerVision
    {
        private readonly Transform _eyesTransform;
        private readonly RadarConfig _config;
        

        private readonly HashSet<Collider> _visibleEnemies = new HashSet<Collider>();
        

        private readonly Collider[] _hitColliders = new Collider[20]; 

        public PlayerVision(Transform eyesTransform, RadarConfig config)
        {
            _eyesTransform = eyesTransform;
            _config = config;
        }

        public void Tick()
        {
            int count = Physics.OverlapSphereNonAlloc(
                _eyesTransform.position, 
                _config.sightRadius, 
                _hitColliders, 
                _config.enemyLayer
            );

            HashSet<Collider> currentFrameEnemies = new HashSet<Collider>();

            for (int i = 0; i < count; i++)
            {
                Collider col = _hitColliders[i];
                currentFrameEnemies.Add(col);

                if (!_visibleEnemies.Contains(col))
                {
                    _visibleEnemies.Add(col);
                    
                    IVisibleTarget visibleTarget = col.GetComponentInParent<IVisibleTarget>();
                    if (visibleTarget != null)
                    {
                        visibleTarget.SetVisibility(true);
                    }
                }
            }

            _visibleEnemies.RemoveWhere(col => 
            {

                if (!currentFrameEnemies.Contains(col))
                {

                    if (col != null)
                    {
                        IVisibleTarget visibleTarget = col.GetComponentInParent<IVisibleTarget>();
                        if (visibleTarget != null)
                        {
                            visibleTarget.SetVisibility(false);
                        }
                    }
                    return true; 
                }
                return false;
            });
        }
    }
}