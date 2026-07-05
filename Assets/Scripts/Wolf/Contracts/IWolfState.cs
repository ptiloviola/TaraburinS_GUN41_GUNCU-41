namespace MeatMushrooms.Wolf.Contracts
{
    public interface IWolfState
    {
        float CalculateScore(); 

        void Enter();
        
        void Tick(); 
        
        void Exit();
    }
}