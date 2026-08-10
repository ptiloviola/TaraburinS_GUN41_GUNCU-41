using UnityEngine;

namespace TpsShooter.Effects
{
    public class DecalManager : MonoBehaviour
    {
        [Header("Pool Settings")]
        [SerializeField] private GameObject _decalPrefab;
        [SerializeField] private int _poolSize = 50;

        private GameObject[] _decals;
        private int _currentIndex = 0;

        private void Awake()
        {
            if (_decalPrefab == null)
            {
                Debug.LogError("[DecalManager] Не назначен префаб декали!");
                return;
            }

            // Предсоздаем все декали при старте сцены
            _decals = new GameObject[_poolSize];
            for (int i = 0; i < _poolSize; i++)
            {
                _decals[i] = Instantiate(_decalPrefab, transform);
                _decals[i].SetActive(false);
            }
        }

        public void SpawnDecal(Vector3 position, Vector3 normal, Transform parent)
        {
            if (_decals == null || _decals.Length == 0) return;

            // Берем самую старую декаль из пула
            GameObject decal = _decals[_currentIndex];
            
            // Настраиваем её позицию и вращение (-normal разворачивает её "лицом" от стены)
            decal.transform.position = position + normal * 0.001f;
            decal.transform.rotation = Quaternion.LookRotation(-normal);
            decal.transform.SetParent(parent);
            
            decal.SetActive(true);

            // Сдвигаем индекс по кругу. После 49-й декали снова возьмем 0-ю.
            _currentIndex = (_currentIndex + 1) % _poolSize;
        }
    }
}