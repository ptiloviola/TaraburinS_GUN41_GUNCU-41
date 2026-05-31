using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

namespace Gameplay.Enemies.Visuals
{
    public class JumperAnimator : MonoBehaviour
    {
        [Header("Настройки времени и высоты")]
        [SerializeField] private Transform _visualMesh;
        [SerializeField] private float _jumpDuration = 0.6f; // Время самого прыжка (полет)
        [SerializeField] private float _pauseDuration = 0.4f; // НОВОЕ: Время отдыха на земле между прыжками
        [SerializeField] private float _jumpHeight = 1.5f;
        
        [Header("Настройки деформации")]
        [SerializeField] private float _squashAmount = 0.5f;
        [SerializeField] private float _stretchAmount = 1.5f;

        private NavMeshAgent _agent;
        private Sequence _jumpSequence;
        private Vector3 _initialScale;
        private Vector3 _initialLocalPos;

        private void Awake()
        {
            if (_visualMesh == null) _visualMesh = transform.Find("Visual");
            _agent = GetComponent<NavMeshAgent>();

            if (_visualMesh != null)
            {
                _initialScale = _visualMesh.localScale;
                _initialLocalPos = _visualMesh.localPosition; 
            }
        }

        private void OnEnable()
        {
            if (_visualMesh != null)
            {
                _visualMesh.localScale = _initialScale;
                _visualMesh.localPosition = _initialLocalPos;
                _visualMesh.localRotation = Quaternion.identity;

                StartJumpAnimation();
            }
        }

        private void StartJumpAnimation()
        {
            _jumpSequence = DOTween.Sequence();

            Vector3 squashScale = new Vector3(_initialScale.x * 1.3f, _initialScale.y * _squashAmount, _initialScale.z * 1.3f);
            Vector3 stretchScale = new Vector3(_initialScale.x * 0.8f, _initialScale.y * _stretchAmount, _initialScale.z * 0.8f);

            // Для удобства разобьем время на логические отрезки
            float prepTime = _jumpDuration * 0.15f;
            float halfFlight = _jumpDuration * 0.35f;
            float impactTime = _jumpDuration * 0.15f;

            // 1. СЖАТИЕ ПЕРЕД ПРЫЖКОМ. Агент стоит на месте.
            _jumpSequence.AppendCallback(() => SetAgentMovement(false));
            _jumpSequence.Append(_visualMesh.DOScale(squashScale, prepTime).SetEase(Ease.InOutQuad));

            // 2. ВЗЛЕТ И ПОЛЕТ. Даем команду агенту двигаться!
            _jumpSequence.AppendCallback(() => SetAgentMovement(true));
            _jumpSequence.Append(_visualMesh.DOScale(stretchScale, halfFlight).SetEase(Ease.OutSine));
            _jumpSequence.Join(_visualMesh.DOLocalMoveY(_initialLocalPos.y + _jumpHeight, halfFlight).SetEase(Ease.OutQuad));

            // 3. ПАДЕНИЕ. Агент всё еще движется. Возвращаем нормальный масштаб в воздухе.
            _jumpSequence.Append(_visualMesh.DOScale(_initialScale, halfFlight).SetEase(Ease.InSine));
            _jumpSequence.Join(_visualMesh.DOLocalMoveY(_initialLocalPos.y, halfFlight).SetEase(Ease.InQuad));

            // 4. ПРИЗЕМЛЕНИЕ И СЖАТИЕ ОТ УДАРА. Тормозим агента.
            _jumpSequence.AppendCallback(() => SetAgentMovement(false));
            _jumpSequence.Append(_visualMesh.DOScale(squashScale, impactTime).SetEase(Ease.OutQuad));
            
            // 5. ВЫПРЯМЛЕНИЕ. Возврат в исходную форму.
            _jumpSequence.Append(_visualMesh.DOScale(_initialScale, impactTime).SetEase(Ease.OutBack));

            // 6. ПАУЗА. Стоим ровно на месте и ждем перед новым циклом.
            _jumpSequence.AppendInterval(_pauseDuration);

            _jumpSequence.SetLoops(-1, LoopType.Restart);
        }

        private void SetAgentMovement(bool canMove)
        {
            if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = !canMove;
            }
        }

        private void OnDisable()
        {
            if (_jumpSequence != null)
            {
                _jumpSequence.Kill();
            }
        }
    }
}