using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace MeatMushrooms.Mushroom.Components
{
    public class MushroomAnimator : MonoBehaviour
    {
        private Vector3 _originalScale;

        private void Awake()
        {
            // Запоминаем нормальный размер гриба
            _originalScale = transform.localScale;
            // Прячем его в нулевой скейл при появлении объекта
            transform.localScale = Vector3.zero; 
        }

        public void PlaySpawnAnimation()
        {
            // Эффект "выпрыгивания" из-под земли (Ease.OutBack дает легкий отскок)
            transform.DOScale(_originalScale, 0.5f).SetEase(Ease.OutBack);
        }

        public async UniTask PlayDeathAnimation()
        {
            // Схлопываем гриб обратно в ноль. Ждем окончания анимации.
            await transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).AsyncWaitForCompletion();
        }
    }
}