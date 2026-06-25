using VacuumSim.Trash;

namespace VacuumSim.Robotics.Signals
{
    // Сигнал: Мусор был успешно всосан
    public struct TrashCollectedSignal
    {
        // Передаем данные о мусоре, чтобы системы знали его вес и цену
        public TrashType TrashData; 
    }
}

