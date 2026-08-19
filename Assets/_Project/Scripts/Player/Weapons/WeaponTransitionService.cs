using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace TpsShooter.Player.Weapons
{
    public class WeaponTransitionService
    {
        // MonoBehaviour больше не нужен!
        public WeaponTransitionService() { }

        public async UniTask MoveWeaponToSocketAsync(Transform weaponTransform, Transform targetSocket, float duration, CancellationToken cancelToken)
        {
            weaponTransform.SetParent(targetSocket, true);
            Vector3 startPos = weaponTransform.localPosition;
            Quaternion startRot = weaponTransform.localRotation;
            float elapsed = 0f;

            try
            {
                while (elapsed < duration)
                {
                    cancelToken.ThrowIfCancellationRequested(); // ЖЕСТКОЕ ТРЕБОВАНИЕ ТЗ: проверка отмены

                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    float smoothT = Mathf.SmoothStep(0f, 1f, t);

                    weaponTransform.localPosition = Vector3.Lerp(startPos, Vector3.zero, smoothT);
                    weaponTransform.localRotation = Quaternion.Lerp(startRot, Quaternion.identity, smoothT);

                    await UniTask.Yield(PlayerLoopTiming.Update, cancelToken);
                }

                weaponTransform.localPosition = Vector3.zero;
                weaponTransform.localRotation = Quaternion.identity;
            }
            catch (OperationCanceledException)
            {
                // ЖЕСТКОЕ ТРЕБОВАНИЕ ТЗ: осознанная обработка отмены
                Debug.LogWarning("[WeaponTransition] Смена оружия прервана (игрок умер или переключил пушку)!");
                weaponTransform.localPosition = Vector3.zero;
                weaponTransform.localRotation = Quaternion.identity;
                throw; 
            }
        }
    }
}