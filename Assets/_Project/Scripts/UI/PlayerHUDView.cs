using UnityEngine;
using TMPro;

namespace TpsShooter.UI
{
    public class PlayerHUDView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private TextMeshProUGUI _ammoText;

        public void UpdateHealth(float current, float max)
        {
            _healthText.text = $"HP: {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }

        public void UpdateAmmo(int inClip, int inReserve)
        {
            _ammoText.text = $"AMMO: {inClip} / {inReserve}";
        }
    }
}