using UnityEngine;

namespace TpsShooter.Effects
{
    public class DecalManager : MonoBehaviour
    {
        [Header("Pool Settings")]
        [SerializeField] private GameObject _decalPrefab;
        [SerializeField] private int _poolSize = 50;
        
        [Tooltip("Отступ от нормали стены, чтобы избежать мерцания текстур (Z-fighting)")]
        [SerializeField] private float _wallOffset = 0.01f; 

        private GameObject[] _decals;
        private int _currentIndex = 0;

        private void Awake()
        {
            if (_decalPrefab == null)
            {
                DevLogger.LogError("[DecalManager] Не назначен префаб декали!");
                return;
            }

            _decals = new GameObject[_poolSize];
            for (int i = 0; i < _poolSize; i++)
            {
                CreateDecalInstance(i);
            }
        }

        private void CreateDecalInstance(int index)
        {
            _decals[index] = Instantiate(_decalPrefab, transform);
            _decals[index].SetActive(false);
        }

        public void SpawnDecal(Vector3 position, Vector3 normal, Transform parent)
        {
            if (_decals == null || _decals.Length == 0) return;

            if (_decals[_currentIndex] == null)
            {
                CreateDecalInstance(_currentIndex);
            }

            GameObject decal = _decals[_currentIndex];
            
            decal.transform.SetParent(null); 
            
            decal.transform.position = position + normal * _wallOffset;
            decal.transform.rotation = Quaternion.LookRotation(-normal);
            
            decal.transform.SetParent(parent, true);
            decal.SetActive(true);

            _currentIndex = (_currentIndex + 1) % _poolSize;
        }
    }
}