namespace MeatMushrooms.Wolf.Contracts
{
    public interface IWolfState
    {
        // Метод для оценки: насколько сильно волк хочет делать это ПРЯМО СЕЙЧАС (от 0 до 100)
        float CalculateScore(); 

        void Enter();
        
        // Метод будет вызываться каждый кадр, пока состояние активно
        void Tick(); 
        
        void Exit();
    }
}