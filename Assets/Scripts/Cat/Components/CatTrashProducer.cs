using UnityEngine;
using Zenject;
using VacuumSim.Pathfinding;
using VacuumSim.Trash;
using VacuumSim.Cat.Contracts;

namespace VacuumSim.Cat.Components
{
    public class CatTrashProducer : MonoBehaviour, ICatTrashProducer
    {
        [Inject] private PathfindingGrid _grid;
        [SerializeField] private TrashType _catTrashType;
        [SerializeField] private float _heightOffset = 1.0f;

        public void ProduceTrash(Vector3 position)
        {
            if (_catTrashType == null || _catTrashType.Prefab == null || _grid == null) return;

            Vector3 spawnPos = new Vector3(position.x, _grid.transform.position.y + _heightOffset, position.z);
            Instantiate(_catTrashType.Prefab, spawnPos, Quaternion.identity);

            Node currentNode = _grid.NodeFromWorldPoint(spawnPos);
            if (currentNode != null)
            {
                currentNode.HasTrash = true;
                currentNode.IsCleaned = false; 
            }
            
            Debug.Log($"<color=magenta>[CatTrash] Кот нагадил: {_catTrashType.Title}</color>");
        }
    }
}