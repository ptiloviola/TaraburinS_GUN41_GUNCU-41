using System;
using UnityEngine;
using UnityEngine.UI;

namespace TpsShooter.UI.Settings
{
    public class SettingsUIView : MonoBehaviour
    {
        [Header("Sliders")]
        [SerializeField] private Slider _masterSlider;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;

        [Header("Buttons")]
        [SerializeField] private Button _closeButton;

        public event Action<float> OnMasterVolumeChanged;
        public event Action<float> OnMusicVolumeChanged;
        public event Action<float> OnSFXVolumeChanged;
        public event Action OnCloseClicked;

        private void Awake()
        {
            _masterSlider.onValueChanged.AddListener(val => OnMasterVolumeChanged?.Invoke(val));
            _musicSlider.onValueChanged.AddListener(val => OnMusicVolumeChanged?.Invoke(val));
            _sfxSlider.onValueChanged.AddListener(val => OnSFXVolumeChanged?.Invoke(val));
            _closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
        }

        public void InitializeSliders(float master, float music, float sfx)
        {
            _masterSlider.SetValueWithoutNotify(master);
            _musicSlider.SetValueWithoutNotify(music);
            _sfxSlider.SetValueWithoutNotify(sfx);
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        private void OnDestroy()
        {
            _masterSlider.onValueChanged.RemoveAllListeners();
            _musicSlider.onValueChanged.RemoveAllListeners();
            _sfxSlider.onValueChanged.RemoveAllListeners();
            _closeButton.onClick.RemoveAllListeners();
        }
    }
}