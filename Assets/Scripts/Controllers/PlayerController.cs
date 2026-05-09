using System.Collections;
using UnityEngine;
using Zenject;

public class PlayerController : MonoBehaviour
{
    private ISharedData _data;
    private SignalBus _signal;
    private ITurn _turn;

    private ChessRuleValidator _chessRuleValidator;


    [Inject]
    public void Construct(ISharedData data, SignalBus signal, ITurn turn, ChessRuleValidator chessRuleValidator)
    {
        _data = data;
        _signal = signal;
        _turn = turn;
        _signal.Subscribe<GameStatus>(OnStatusChanged);
        _chessRuleValidator = chessRuleValidator;
    }

    private void OnStatusChanged(GameStatus status)
    {
        if (status == GameStatus.Confirm)
        {
            StartCoroutine(ProcessTurnEnd());  
        }
    }

    private IEnumerator ProcessTurnEnd()
    {
        _data.Lock = true;
        Debug.Log("<color=red>[PlayerController] Ввод заблокирован. Ждем завершения анимации хода...</color>");

        // Ждем секунду для имитации движения (позже привяжем к реальному завершению движения)
        yield return new WaitForSeconds(1.0f);

        _turn.Next();

        if (_chessRuleValidator.IsKingOnCheck(_turn.Current))
        {
            if (_chessRuleValidator.IsCheckMate(_turn.Current))
            {
                Debug.Log($"<color=black>Досвидули, {_turn.Current}. МАТ! </color>");
                _signal.Fire(new CheckSignal {TeamInCheck = _turn.Current, IsCheckMate = true});
            }
            else
            {
               Debug.Log($"<color=red>ОПАЧКИ! Шах команде {_turn.Current}!</color>");
                _signal.Fire(new CheckSignal {TeamInCheck = _turn.Current, IsCheckMate = false}); 
            }
            
        }

        _data.Lock = false;
        _data.Status = GameStatus.Select;

        Debug.Log($"<color=blue>[PlayerController] Ход передан. Сейчас ходит: {_turn.Current}. Ввод разблокирован.</color>");

    }

    private void OnDestroy()
    {
        _signal.Unsubscribe<GameStatus>(OnStatusChanged);
    }

}
