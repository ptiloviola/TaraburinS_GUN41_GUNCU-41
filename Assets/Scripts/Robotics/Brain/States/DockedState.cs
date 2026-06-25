using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VacuumSim.Robotics.Contracts;

namespace VacuumSim.Robotics.Brain.States
{
    public class DockedState : IVacuumState
    {
        private readonly IVacuumBattery _battery;
        private readonly IVacuumDustbin _dustbin;

        // Скорость зарядки (единиц в секунду). Можно вынести в Config.
        private const float ChargeRate = 15f; 

        public DockedState(IVacuumBattery battery, IVacuumDustbin dustbin)
        {
            _battery = battery;
            _dustbin = dustbin;
        }

        public async UniTask ExecuteAsync(CancellationToken token)
        {
            Debug.Log("[State] Робот на станции. Начинаю обслуживание...");

            // 1. Мгновенно вытряхиваем бак

            _dustbin.EmptyBin();
            // Небольшая пауза для реализма (имитация звука вытряхивания)
            await UniTask.Delay(1000, cancellationToken: token); 


            // 2. Плавно заряжаем батарею
            while (!_battery.IsFull && !token.IsCancellationRequested)
            {
                _battery.Charge(ChargeRate * Time.deltaTime);
                
                // Ждем до следующего кадра
                await UniTask.Yield(token);
            }

            Debug.Log("[State] Обслуживание завершено! Робот готов к работе.");
            // Робот остается в этом состоянии (спит), пока мы не нажмем кнопку в UI
        }
    }
}