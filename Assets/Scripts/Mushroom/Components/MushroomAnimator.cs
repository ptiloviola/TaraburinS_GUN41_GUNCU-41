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
            _originalScale = transform.localScale;
            transform.localScale = Vector3.zero; 
        }

        public void PlaySpawnAnimation()
        {
            transform.DOScale(_originalScale, 0.5f).SetEase(Ease.OutBack);
        }

        public async UniTask PlayDeathAnimation()
        {
            await transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).AsyncWaitForCompletion();
        }
    }
}