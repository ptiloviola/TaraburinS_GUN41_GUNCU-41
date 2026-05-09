using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class OneByOneTurn : ITurn
{
    private int _index;
    private readonly IReadOnlyList<Team> _teams;

    private readonly SignalBus _signal;

    public Team Current => _teams[_index];

    public void Next()
    {
        Debug.Log($"<color=cyan>[OneByOneTurn] Переход хода. Теперь ходят: {Current}</color>");
        _index = (_index + 1) % _teams.Count;
        _signal.Fire<TurnChangedSignal>();
    }

    public OneByOneTurn(IReadOnlyList<Team> teams, SignalBus signal)
    {
        _teams = teams;
        _index = 0;
        _signal = signal;
    }
}
