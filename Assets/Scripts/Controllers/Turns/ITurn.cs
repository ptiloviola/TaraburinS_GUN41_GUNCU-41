using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITurn
{
    Team Current { get; }
    void Next();

}
