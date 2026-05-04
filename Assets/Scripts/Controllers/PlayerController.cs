using System.Collections;
using UnityEngine;
using Zenject;

public class PlayerController : MonoBehaviour
{
    private ISharedData _data;
    private SignalBus _signal;
    private ITurn _turn;

    [Inject]
    public void Construct(ISharedData data, SignalBus signal, ITurn turn)
    {
        _data = data;
        _signal = signal;
        _turn = turn;
        _signal.Subscribe<GameStatus>(OnStatusChanged);
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
        _data.Lock = false;
        _data.Status = GameStatus.Select;

        Debug.Log($"<color=blue>[PlayerController] Ход передан. Сейчас ходит: {_turn.Current}. Ввод разблокирован.</color>");

    }

    private void OnDestroy()
    {
        _signal.Unsubscribe<GameStatus>(OnStatusChanged);
    }

}
