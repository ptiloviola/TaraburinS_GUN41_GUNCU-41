using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

namespace Gameplay.Enemies.Visuals
{
    public class SlinkyAnimator : MonoBehaviour
    {
        [Header("Настройки анимации")]
        [SerializeField] private Transform _visualMesh;
        [SerializeField] private float _stepDuration = 1f; // Время одного кувырка
        [SerializeField] private float _stretchMultiplier = 2f; // Во сколько раз растягивается пружина

        private NavMeshAgent _agent;
        private Sequence _slinkySequence;

        private Vector3 _initialScale;

        private void Awake()
        {
            if (_visualMesh == null) _visualMesh = transform.Find("Visual");
            _agent = GetComponent<NavMeshAgent>();
            
            // Запоминаем масштаб (твой 0.1, 0.1, 0.1), который ты выставил в инспекторе
            if (_visualMesh != null) _initialScale = _visualMesh.localScale; 
        }

        private void OnEnable()
        {
            if (_visualMesh != null)
            {
                _visualMesh.localRotation = Quaternion.identity;
                // Возвращаем правильный масштаб!
                _visualMesh.localScale = _initialScale; 
                
                StartSlinkyAnimation();
            }
        }

        private void StartSlinkyAnimation()
        {
            // 1. Создаем пустую секвенцию
            _slinkySequence = DOTween.Sequence();

            // 2. Добавляем кувырок вперед на 180 градусов (LocalAxisAdd значит "прибавить 180 к текущему углу")
            _slinkySequence.Append(_visualMesh.DORotate(
                new Vector3(180f, 0, 0), 
                _stepDuration, 
                RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutSine)); // Плавный старт и конец вращения

            // 3. ПАРАЛЛЕЛЬНО (Join) растягиваем башню
            // Yoyo означает, что анимация проиграется вперед (растянется) и сразу назад (сожмется).
            // Поэтому время мы делим на 2 (_stepDuration / 2f).
            _slinkySequence.Join(_visualMesh.DOScale(
                new Vector3(1f, _stretchMultiplier, 1f), 
                _stepDuration / 2f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutQuad));

            // 4. Зацикливаем бесконечно
            _slinkySequence.SetLoops(-1, LoopType.Restart);
        }

        private void Update()
        {
            // Небольшая полировка: если агент стоит на месте, ставим анимацию на паузу
            if (_agent != null && _slinkySequence != null)
            {
                if (_agent.velocity.magnitude > 0.1f)
                {
                    _slinkySequence.Play();
                }
                else
                {
                    _slinkySequence.Pause();
                }
            }
        }

        private void OnDisable()
        {
            // ОБЯЗАТЕЛЬНО убиваем Tween при возврате врага в пул, иначе будет утечка памяти!
            if (_slinkySequence != null)
            {
                _slinkySequence.Kill();
            }
        }
    }
}