using UnityEngine;
using UnityEngine.Audio;

namespace Services
{
    public class AudioService
    {
        private readonly AudioMixerGroup _sfxGroup;

        public AudioService(AudioMixerGroup sfxGroup)
        {
            _sfxGroup = sfxGroup;
        }

        // Метод 1: Проигрывание звука из конкретного источника (например, в голове игрока)
        public void PlaySound(AudioSource source, AudioClip clip, bool randomizePitch = false)
        {
            if (clip == null || source == null) return;

            source.outputAudioMixerGroup = _sfxGroup;
            source.pitch = randomizePitch ? Random.Range(0.9f, 1.1f) : 1f;
            source.PlayOneShot(clip);
        }

        // Метод 2: Проигрывание 3D-звука в конкретной точке пространства (например, попадание в стену)
        public void PlaySoundAtPoint(AudioClip clip, Vector3 position, bool randomizePitch = false)
        {
            if (clip == null) return;

            // Создаем временный объект для звука (в реальном проекте здесь используется Object Pool)
            GameObject tempAudioObj = new GameObject("TempAudio_" + clip.name);
            tempAudioObj.transform.position = position;
            
            AudioSource tempSource = tempAudioObj.AddComponent<AudioSource>();
            tempSource.clip = clip;
            tempSource.spatialBlend = 1f; // Делаем звук полностью 3D

            tempSource.outputAudioMixerGroup = _sfxGroup;
            tempSource.pitch = randomizePitch ? Random.Range(0.9f, 1.1f) : 1f;
            
            // Позже сюда добавим роутинг в микшер
            
            tempSource.Play();
            
            // Уничтожаем объект, когда звук закончится
            Object.Destroy(tempAudioObj, clip.length + 0.1f);
        }
    }
}