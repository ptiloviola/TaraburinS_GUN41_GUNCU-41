using UnityEngine;

namespace Bowling.UI.MVP
{

    [System.Serializable]
    public class BallConfig
    {
        public string Name;
        public Sprite Icon;         
        public GameObject BallPrefab;
        public float Mass = 10f;
    }

    public class BallModel : MonoBehaviour
    {
        [Header("База доступных шаров")]
        [SerializeField] private BallConfig[] _availableBalls;

        public BallConfig[] AvailableBalls => _availableBalls;

        public BallConfig GetBall(int index)
        {
            if (index >= 0 && index < _availableBalls.Length)
                return _availableBalls[index];
            return null;
        }
    }
}
