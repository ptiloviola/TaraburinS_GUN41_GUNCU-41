namespace MeatMushrooms.Wolf.Signals
{
    // Мы делаем их структурами для максимальной оптимизации.
    // Пока они пустые, но в будущем внутрь можно положить данные 
    // (например, громкость или координаты источника звука).
    
    public struct WolfHowlSignal { }         // Вой (заметил игрока)
    public struct WolfCombatGrowlSignal { }  // Громкое рычание (в бою/погоне)
    public struct WolfLowGrowlSignal { }     // Тихое рычание (подозрение)
    public struct WolfEatSignal { }          // Чавканье (ест гриб/мясо)
}


