using System;
using System.Collections;
using UnityEngine;

namespace TpsShooter.Player.Weapons
{
    public class WeaponTransitionService
    {
        private readonly MonoBehaviour _coroutineRunner;

        // Передаем сюда Фасад, чтобы чистый класс мог запускать корутины
        public WeaponTransitionService(MonoBehaviour coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
        }

        public void MoveWeaponToSocket(Transform weaponTransform, Transform targetSocket, float duration, Action onComplete = null)
        {
            // SetParent(..., true) — это магия! Оружие сохранит свои мировые координаты (например, на земле),
            // но станет дочерним к сокету. Дальше мы просто плавно сведем его локальные координаты к нулю.
            weaponTransform.SetParent(targetSocket, true);
            _coroutineRunner.StartCoroutine(TransitionRoutine(weaponTransform, duration, onComplete));
        }

        private IEnumerator TransitionRoutine(Transform weaponTransform, float duration, Action onComplete)
        {
            Vector3 startPos = weaponTransform.localPosition;
            Quaternion startRot = weaponTransform.localRotation;
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // SmoothStep делает анимацию мягкой в начале и в конце (ease-in-out)
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                weaponTransform.localPosition = Vector3.Lerp(startPos, Vector3.zero, smoothT);
                weaponTransform.localRotation = Quaternion.Lerp(startRot, Quaternion.identity, smoothT);
                
                yield return null;
            }

            // Жестко фиксируем в конце, чтобы не было микро-зазоров
            weaponTransform.localPosition = Vector3.zero;
            weaponTransform.localRotation = Quaternion.identity;
            
            onComplete?.Invoke();
        }
    }
}