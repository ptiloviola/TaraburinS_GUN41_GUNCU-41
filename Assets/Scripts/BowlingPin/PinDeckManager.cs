using System.Collections.Generic;
using UnityEngine;

namespace Bowling.BowlingPins
{
    public class PinDeckManager : MonoBehaviour
    {
        [Header("Настройки генерации")]
        [SerializeField] private GameObject _pinPrefab;
        
        [Tooltip("Количество рядов")]
        [SerializeField] private int _rowCount = 4; 
        
        [Tooltip("Расстояние между центрами соседних кеглей.")]
        [SerializeField] private float _spacing = 0.3f;

        public int FullDeckSize { get; private set; }


        public List<BowlingPin> ActivePins { get; private set; } = new List<BowlingPin>();

        [ContextMenu("Сгенерировать пирамиду (Spawn)")]
        public void SpawnPins()
        {
            ClearPins();
            if (_pinPrefab == null)
            {
                Debug.LogError("Не назначен префаб кегли в PinDeckManager!");
                return;
            }

            float rowHeightOffset = Mathf.Sqrt(3f) / 2f;

            for (int row = 0; row < _rowCount; row++)
            {

                float zPos = row * _spacing * rowHeightOffset;

                for (int col = 0; col <= row; col++)
                {
                    float xPos = (col * _spacing) - ((row * _spacing) / 2f);
                    Vector3 localSpawnPosition = new Vector3(xPos, 0f, zPos);
                    Vector3 worldSpawnPosition = transform.TransformPoint(localSpawnPosition);
                    GameObject newPin = Instantiate(_pinPrefab, worldSpawnPosition, transform.rotation, transform);
                    newPin.name = $"Pin_{row}_{col}";
                    if (newPin.TryGetComponent<BowlingPin>(out BowlingPin pinComponent))
                    {
                        ActivePins.Add(pinComponent);
                    }
                }
            }
            FullDeckSize = ActivePins.Count;
        }
        [ContextMenu("Удалить все кегли (Clear)")]
        public void ClearPins()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                
                if (Application.isPlaying)
                {
                    child.SetActive(false);
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
            
            ActivePins.Clear();
        }

        public void ResetDeck()
        {
            SpawnPins();
        }

        public void RemoveFallenPins()
        {
            for (int i = ActivePins.Count - 1; i >= 0; i--)
            {
                BowlingPin pin = ActivePins[i];
                if (pin != null && pin.IsFallen())
                {
                    pin.gameObject.SetActive(false);
                    Destroy(pin.gameObject);
                    ActivePins.RemoveAt(i);
                }
                else if (pin != null)
                {
                    pin.ResetPin();
                }
            }
        }

        public bool AreAllPinsSettled()
        {
            foreach (var pin in ActivePins)
            {
                if (pin != null)
                {
                    if (pin.TryGetComponent<Rigidbody>(out Rigidbody rb))
                    {
                        if (rb.velocity.magnitude > 0.05f || rb.angularVelocity.magnitude > 0.05f)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}