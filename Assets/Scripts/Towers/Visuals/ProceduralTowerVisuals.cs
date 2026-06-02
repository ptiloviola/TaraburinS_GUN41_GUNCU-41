using UnityEngine;
using DG.Tweening;
using System;

namespace Gameplay.Towers.Visuals
{
    public class ProceduralTowerVisuals : MonoBehaviour, ITowerVisuals
    {
        [Header("--- Ссылки на меши ---")]
        [SerializeField] private Transform _baseTransform;
        [SerializeField] private Transform _turretTransform;

        [SerializeField] private Transform[] _barrelTransforms;

        [Header("--- Связь с Логикой ---")]
        [Tooltip("Если пусто, скрипт попытается найти объект 'Logical_Rotator' автоматически у родителя.")]
        [SerializeField] private Transform _logicalRotator; 
        [Header("--- Зенитная механика (Двухосевая) ---")]
        [Tooltip("Включите для зениток")]
        [SerializeField] private bool _copyPitch = false;

        [Tooltip("Ось вертикального наклона (Pitch). Сюда кидаем нашу новую пустышку Elevation_Pivot")]
        [SerializeField] private Transform _elevationPivot;

        [Tooltip("Локальная ось наклона. Так как башня из Blender повернута на -90 (смотрит по X), осью подъема стволов будет ось Z. Впишите: X=0, Y=0, Z=1 (или -1)")]
        [SerializeField] private Vector3 _localPitchAxis = new Vector3(0, 0, -1);

        [Tooltip("Смещение поворота в градусах. Помогает направить ствол из Blender (ось X) вслед за логикой Unity (ось Z). Попробуйте -90 или 90.")]
        [SerializeField] private float _rotationOffset = -90f; // <--- НАШ СПАСИТЕЛЬНЫЙ ОФСЕТ

        [Header("--- Настройки Появления (Build) ---")]
        [Range(0.1f, 2f)] [SerializeField] private float _buildDuration = 0.5f;
        [SerializeField] private Ease _buildEase = Ease.OutBack;

        [Header("--- Настройки Выстрела (Shoot) ---")]
        [Range(0.05f, 1f)] [SerializeField] private float _shootDuration = 0.2f;
        [Range(0.1f, 2f)] [SerializeField] private float _recoilDistance = 0.4f;
        [Range(0.5f, 1f)] [SerializeField] private float _turretSquashY = 0.85f;

        [Header("--- Настройки Плавности (Ease) ---")]
        [SerializeField] private Ease _recoilEase = Ease.OutQuad;
        [SerializeField] private Ease _barrelReturnEase = Ease.OutQuad;
        [SerializeField] private Ease _turretReturnEase = Ease.OutQuad;
        
        [Header("--- Эффекты ---")]
        [SerializeField] private LineRenderer _laserRenderer;
        [SerializeField] private float _laserMaxWidth = 0.3f;

        // 1. ДОБАВЛЯЕМ МАССИВ ДЛЯ НАШИХ ЧАСТИЦ (По одному на ствол)
        [Tooltip("Перетащите сюда Particle System пара из каждого ствола в том же порядке, что и сами стволы")]
        [SerializeField] private ParticleSystem[] _steamParticles;




        // private Sequence _shootSequence;
        // Вместо одного _shootSequence у нас теперь два независимых контроллера:
        private Sequence[] _barrelSequences; // Массив личных секвенций для каждого ствола
        private Sequence _sharedEffectsSequence; // Общая секвенция для Головы и Лазера


        private Vector3[] _initialBarrelLocalPositions;

        // Добавь эту переменную в самый верх класса к остальным приватным полям:
        private int _currentBarrelIndex = 0;

        // Единая переменная для хранения целевого угла
        private float _targetYRotation;

        public event Action OnAttackImpact;

        public void Initialize()
        {
            if (_barrelTransforms != null && _barrelTransforms.Length > 0)
            {
                // 1. Выделяем память под массив векторов ровно такой же длины, 
                // сколько стволов перетащили в инспектор
                _initialBarrelLocalPositions = new Vector3[_barrelTransforms.Length];
                // ВЫДЕЛЯЕМ ПАМЯТЬ ПОД МАССИВ СЕКВЕНЦИЙ СТВОЛОВ:
                _barrelSequences = new Sequence[_barrelTransforms.Length];

                for (int i = 0; i < _barrelTransforms.Length; i++)
                {
                    if (_barrelTransforms[i] != null)
                    {
                        _initialBarrelLocalPositions[i] = _barrelTransforms[i].localPosition;
                    }
                }
            }

            // АВТОПОИСК: Если ссылка в инспекторе пустая, спасаем ситуацию кодом
            if (_logicalRotator == null && transform.parent != null)
            {
                // Ищем соседа с именем "Logical_Rotator" через общего родителя префаба
                _logicalRotator = transform.parent.Find("Logical_Rotator");
            }

            ResetScaleToZero();
        }

        public void PlayBuildAnimation()
        {
            ResetScaleToZero();
            Sequence buildSequence = DOTween.Sequence();

            if (_baseTransform != null)
            {
                buildSequence.Append(_baseTransform.DOScale(Vector3.one, _buildDuration).SetEase(_buildEase));
            }
            if (_turretTransform != null)
            {
                buildSequence.Insert(_buildDuration * 0.4f, _turretTransform.DOScale(Vector3.one, _buildDuration).SetEase(_buildEase));
            }
        }

        public void PlayShootAnimation(Vector3 targetPosition)
        {
            if (_barrelTransforms == null || _barrelTransforms.Length == 0) return;

            Transform activeBarrel = _barrelTransforms[_currentBarrelIndex];
            Vector3 activeInitialPos = _initialBarrelLocalPositions[_currentBarrelIndex];

            // 2. ИЩЕМ АКТИВНЫЙ ПАР
            ParticleSystem activeSteam = null;
            if (_steamParticles != null && _currentBarrelIndex < _steamParticles.Length)
            {
                activeSteam = _steamParticles[_currentBarrelIndex];
            }

            // ---------------------------------------------------------
            // 1. ПЕРСОНАЛЬНАЯ АНИМАЦИЯ СТВОЛА (ПОРШНИ)
            // ---------------------------------------------------------
            // Мы прерываем секвенцию ТОЛЬКО того ствола, который стреляет прямо сейчас.
            // Соседний ствол продолжит свое плавное движение!
            if (_barrelSequences[_currentBarrelIndex] != null && _barrelSequences[_currentBarrelIndex].IsActive())
            {
                _barrelSequences[_currentBarrelIndex].Complete();
            }

            _barrelSequences[_currentBarrelIndex] = DOTween.Sequence();
            
            if (activeBarrel != null)
            {
                float recoilX = activeInitialPos.x - _recoilDistance;
                _barrelSequences[_currentBarrelIndex].Append(activeBarrel.DOLocalMoveX(recoilX, _shootDuration * 0.25f).SetEase(_recoilEase));
                _barrelSequences[_currentBarrelIndex].Append(activeBarrel.DOLocalMoveX(activeInitialPos.x, _shootDuration * 0.75f).SetEase(_barrelReturnEase));
                // 3. МАГИЯ СВЯЗКИ: InsertCallback!
                // Мы говорим: "Ровно на 0-й секунде (когда ствол пошел назад), дерни метод Play() у нашей системы частиц"
                if (activeSteam != null)
                {
                    _barrelSequences[_currentBarrelIndex].InsertCallback(0f, () => activeSteam.Play());
                }
            }

            // ---------------------------------------------------------
            // 2. ОБЩАЯ АНИМАЦИЯ (ГОЛОВА И ЛАЗЕР)
            // ---------------------------------------------------------
            // Голова и лазер у нас одни на всю башню, поэтому их старую анимацию мы всегда убиваем
            if (_sharedEffectsSequence != null && _sharedEffectsSequence.IsActive())
            {
                _sharedEffectsSequence.Complete();
            }

            _sharedEffectsSequence = DOTween.Sequence();
            Vector3 mechanicalSquash = new Vector3(1f, _turretSquashY, 1f);

            if (_turretTransform != null)
            {
                _sharedEffectsSequence.Insert(0, _turretTransform.DOScale(mechanicalSquash, _shootDuration * 0.25f).SetEase(_recoilEase));
                _sharedEffectsSequence.Insert(_shootDuration * 0.25f, _turretTransform.DOScale(Vector3.one, _shootDuration * 0.75f).SetEase(_turretReturnEase));
            }

            if (_laserRenderer != null && activeBarrel != null)
            {
                _laserRenderer.enabled = true;
                Vector3 firePointPos = activeBarrel.position + activeBarrel.right * 0.5f;
                _laserRenderer.SetPosition(0, firePointPos);
                _laserRenderer.SetPosition(1, targetPosition + Vector3.up * 0.5f);
                _laserRenderer.widthMultiplier = _laserMaxWidth;

                // Лазер теперь живет в общей секвенции, он никогда не потеряется!
                _sharedEffectsSequence.Insert(0, DOTween.To(
                    () => _laserRenderer.widthMultiplier, 
                    x => _laserRenderer.widthMultiplier = x, 
                    0f, 
                    _shootDuration
                ).SetEase(Ease.OutExpo));

                // Выключаем лазер, когда общая секвенция завершилась
                _sharedEffectsSequence.OnComplete(() => 
                {
                    if (_laserRenderer != null) _laserRenderer.enabled = false;
                });
            }

            // ПЕРЕКЛЮЧАЕМ ИНДЕКС СТВОЛА НА СЛЕДУЮЩИЙ
            _currentBarrelIndex = (_currentBarrelIndex + 1) % _barrelTransforms.Length;
        }

        private void ResetScaleToZero()
        {
            if (_baseTransform != null) _baseTransform.localScale = Vector3.zero;
            if (_turretTransform != null) _turretTransform.localScale = Vector3.zero;
        }

        // ВОЗВРАЩАЕМ МЕТОД НА МЕСТО: Теперь он снова существует и доступен для тестера
        public void SetRotation(float angleY)
        {
            _targetYRotation = angleY;
        }

        private void LateUpdate()
        {
            if (_logicalRotator != null && _turretTransform != null)
            {
                // 1. ГОРИЗОНТАЛЬ (Yaw) - Вращаем только саму платформу (Turret)
                float targetYAngle = _logicalRotator.eulerAngles.y + _rotationOffset;
                _turretTransform.rotation = Quaternion.Euler(0, targetYAngle, 0);

                // 2. ВЕРТИКАЛЬ (Pitch) - Вращаем только крепление стволов (Elevation_Pivot)
                if (_copyPitch && _elevationPivot != null)
                {
                    // Достаем чистый угол наклона из логики (в Unity он хранится в оси X ротатора)
                    float pitch = _logicalRotator.eulerAngles.x;
                    
                    // Приводим угол от формата 0..360 к формату -180..180 (где минус - это ствол вверх)
                    if (pitch > 180f) pitch -= 360f;

                    // Применяем наклон строго по одной локальной оси
                    _elevationPivot.localRotation = Quaternion.Euler(_localPitchAxis * pitch);
                }
            }
        }
    }
}