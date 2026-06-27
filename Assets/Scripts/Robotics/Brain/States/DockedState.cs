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


        private const float ChargeRate = 15f; 

        public DockedState(IVacuumBattery battery, IVacuumDustbin dustbin)
        {
            _battery = battery;
            _dustbin = dustbin;
        }

        public async UniTask ExecuteAsync(CancellationToken token)
        {
            Debug.Log("[State] Робот на станции. Начинаю обслуживание...");

            _dustbin.EmptyBin();

            await UniTask.Delay(1000, cancellationToken: token); 

            while (!_battery.IsFull && !token.IsCancellationRequested)
            {
                _battery.Charge(ChargeRate * Time.deltaTime);

                await UniTask.Yield(token);
            }

            Debug.Log("[State] Обслуживание завершено! Робот готов к работе.");
        }
    }
}