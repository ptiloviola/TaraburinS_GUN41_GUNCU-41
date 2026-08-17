using System;
using UnityEngine;
using UnityEngine.Audio;
using TpsShooter.Audio; // Для AudioConfig

namespace TpsShooter.UI.Settings
{
    public class SettingsPresenter : IDisposable
    {
        private readonly SettingsModel _model;
        private readonly SettingsUIView _view;
        private readonly AudioMixer _mixer;

        public SettingsPresenter(SettingsModel model, SettingsUIView view, AudioConfig audioConfig)
        {
            _model = model;
            _view = view;
            
            // Получаем микшер из глобального конфига
            _mixer = audioConfig != null ? audioConfig.MainMixer : null;

            // Синхронизируем View с Моделью при старте
            _view.InitializeSliders(_model.MasterVolume, _model.MusicVolume, _model.SFXVolume);
            
            // Применяем громкость сразу (чтобы при запуске игры громкость была из сохранений)
            ApplyVolume("MasterVolume", _model.MasterVolume);
            ApplyVolume("MusicVolume", _model.MusicVolume);
            ApplyVolume("SFXVolume", _model.SFXVolume);

            // Подписываемся на события от View
            _view.OnMasterVolumeChanged += HandleMasterChanged;
            _view.OnMusicVolumeChanged += HandleMusicChanged;
            _view.OnSFXVolumeChanged += HandleSFXChanged;
            _view.OnCloseClicked += HandleClose;
        }

        private void HandleMasterChanged(float value)
        {
            _model.MasterVolume = value;
            ApplyVolume("MasterVolume", value);
        }

        private void HandleMusicChanged(float value)
        {
            _model.MusicVolume = value;
            ApplyVolume("MusicVolume", value);
        }

        private void HandleSFXChanged(float value)
        {
            _model.SFXVolume = value;
            ApplyVolume("SFXVolume", value);
        }

        private void HandleClose()
        {
            _model.Save(); // Сохраняем в PlayerPrefs только при закрытии окна
            _view.Hide();
        }

        // Перевод линейной громкости (0-1) в децибелы (логарифмическая шкала)
        private void ApplyVolume(string exposedParameter, float linearValue)
        {
            if (_mixer == null) return;

            // Защита от логарифма нуля (Mathf.Log10(0) = -Infinity)
            float decibels = linearValue > 0.001f ? Mathf.Log10(linearValue) * 20f : -80f;
            _mixer.SetFloat(exposedParameter, decibels);
        }

        public void Dispose()
        {
            _view.OnMasterVolumeChanged -= HandleMasterChanged;
            _view.OnMusicVolumeChanged -= HandleMusicChanged;
            _view.OnSFXVolumeChanged -= HandleSFXChanged;
            _view.OnCloseClicked -= HandleClose;
        }
    }
}