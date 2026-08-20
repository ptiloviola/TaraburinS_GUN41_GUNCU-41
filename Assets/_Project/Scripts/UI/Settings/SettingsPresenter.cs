using System;
using UnityEngine;
using UnityEngine.Audio;
using TpsShooter.Audio; 

namespace TpsShooter.UI.Settings
{
    public class SettingsPresenter : IDisposable
    {
        private const string MasterExposedParam = "MasterVolume";
        private const string MusicExposedParam = "MusicVolume";
        private const string SFXExposedParam = "SFXVolume";

        private readonly SettingsModel _model;
        private readonly SettingsUIView _view;
        private readonly AudioMixer _mixer;

        public SettingsPresenter(SettingsModel model, SettingsUIView view, AudioConfig audioConfig)
        {
            _model = model;
            _view = view;
            
            _mixer = audioConfig != null ? audioConfig.MainMixer : null;

            _view.InitializeSliders(_model.MasterVolume, _model.MusicVolume, _model.SFXVolume);
            
            ApplyVolume(MasterExposedParam, _model.MasterVolume);
            ApplyVolume(MusicExposedParam, _model.MusicVolume);
            ApplyVolume(SFXExposedParam, _model.SFXVolume);

            _view.OnMasterVolumeChanged += HandleMasterChanged;
            _view.OnMusicVolumeChanged += HandleMusicChanged;
            _view.OnSFXVolumeChanged += HandleSFXChanged;
            _view.OnCloseClicked += HandleClose;
        }

        private void HandleMasterChanged(float value)
        {
            _model.MasterVolume = value;
            ApplyVolume(MasterExposedParam, value);
        }

        private void HandleMusicChanged(float value)
        {
            _model.MusicVolume = value;
            ApplyVolume(MusicExposedParam, value);
        }

        private void HandleSFXChanged(float value)
        {
            _model.SFXVolume = value;
            ApplyVolume(SFXExposedParam, value);
        }

        private void HandleClose()
        {
            _model.Save(); 
            _view.Hide();
        }

        private void ApplyVolume(string exposedParameter, float linearValue)
        {
            if (_mixer == null) return;

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