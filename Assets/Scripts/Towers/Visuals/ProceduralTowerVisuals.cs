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
        [SerializeField] private Transform _barrelTransform;

        [Header("--- Связь с Логикой ---")]
        [Tooltip("Если пусто, скрипт попытается найти объект 'Logical_Rotator' автоматически у родителя.")]
        [SerializeField] private Transform _logicalRotator; 

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

        private Sequence _shootSequence;
        private Vector3 _initialBarrelLocalPos;

        // Единая переменная для хранения целевого угла
        private float _targetYRotation;

        public event Action OnAttackImpact;

        public void Initialize()
        {
            if (_barrelTransform != null)
            {
                _initialBarrelLocalPos = _barrelTransform.localPosition;
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

        public void PlayShootAnimation()
        {
            if (_shootSequence != null && _shootSequence.IsActive())
            {
                _shootSequence.Complete();
            }

            _shootSequence = DOTween.Sequence();
            Vector3 mechanicalSquash = new Vector3(1f, _turretSquashY, 1f);

            if (_barrelTransform != null)
            {
                float recoilX = _initialBarrelLocalPos.x - _recoilDistance;
                _shootSequence.Append(_barrelTransform.DOLocalMoveX(recoilX, _shootDuration * 0.25f).SetEase(_recoilEase));
                _shootSequence.Append(_barrelTransform.DOLocalMoveX(_initialBarrelLocalPos.x, _shootDuration * 0.75f).SetEase(_barrelReturnEase));
            }

            if (_turretTransform != null)
            {
                _shootSequence.Insert(0, _turretTransform.DOScale(mechanicalSquash, _shootDuration * 0.25f).SetEase(_recoilEase));
                _shootSequence.Insert(_shootDuration * 0.25f, _turretTransform.DOScale(Vector3.one, _shootDuration * 0.75f).SetEase(_turretReturnEase)); 
            }
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
            // Копируем поворот с учетом компенсации осей из Blender
            if (_logicalRotator != null && _turretTransform != null)
            {
                // Берем чистый угол Y из геймплейной логики и добавляем наш офсет
                float targetYAngle = _logicalRotator.rotation.eulerAngles.y + _rotationOffset;
                
                // Применяем финальный разворот к голове башни
                _turretTransform.rotation = Quaternion.Euler(0, targetYAngle, 0);
            }
        }
    }
}