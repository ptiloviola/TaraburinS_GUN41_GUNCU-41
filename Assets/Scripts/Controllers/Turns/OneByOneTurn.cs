using System.Collections.Generic;
using UnityEngine;

public class OneByOneTurn : ITurn
{
    private int _index;
    private readonly IReadOnlyList<Team> _teams;

    public Team Current => _teams[_index];

    public void Next()
    {
        Debug.Log($"Next turn? {_teams}");
        _index = (_index + 1) % _teams.Count;
    }

    public OneByOneTurn(IReadOnlyList<Team> teams)
    {
        _teams = teams;
        _index = 0;
    }
}
