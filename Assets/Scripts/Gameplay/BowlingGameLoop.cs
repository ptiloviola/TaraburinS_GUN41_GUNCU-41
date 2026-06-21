using UnityEngine;



namespace Bowling.Gameplay
{
    public enum TurnResult
    {
        NextThrow,
        NextFrame,
        GameOver
    }

    public class BowlingGameLoop
    {
        public int CurrentFrame { get; private set; } = 1;
        public int CurrentThrow { get; private set; } = 1;

        private int _throw1 = 0;
        private int _throw2 = 0;

        public TurnResult RegisterThrow(int knockedPins)
        {
            if (CurrentFrame < 10)
            {
                if (CurrentThrow == 1)
                {
                    if (knockedPins == 10)
                    {
                        return AdvanceToNextFrame();
                    }
                    CurrentThrow = 2;
                    return TurnResult.NextThrow;
                }
                else
                {
                    return AdvanceToNextFrame();
                }
            }
            else
            {
                if (CurrentThrow == 1)
                {
                    _throw1 = knockedPins;
                    CurrentThrow = 2;
                    if (_throw1 == 10) return TurnResult.NextFrame;
                    return TurnResult.NextThrow;
    
                }
                else if (CurrentThrow == 2)
                {
                    _throw2 = knockedPins;
                    bool eligibleForBonus = (_throw1 == 10) || (_throw1 + _throw2 == 10);
                    if (eligibleForBonus)
                    {
                        CurrentThrow = 3;
                        if (_throw1 + _throw2 == 10 || _throw2 == 10)
                        {
                            return TurnResult.NextFrame;
                        }
                        return TurnResult.NextThrow;
                    }
                    return TurnResult.GameOver;
                }
                else
                {
                    return TurnResult.GameOver;
                }
            }
        }

        private TurnResult AdvanceToNextFrame()
        {
            CurrentFrame++;
            CurrentThrow = 1;
            return TurnResult.NextFrame;
        }

        public void ResetLoop()
        {
            CurrentFrame = 1;
            CurrentThrow = 1;
            _throw1 = 0;
            _throw2 = 0;
        }
                

    }

    
}

