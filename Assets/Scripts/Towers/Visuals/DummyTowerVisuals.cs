using System;
using Gameplay.Towers.Visuals;
using UnityEngine;

public class DummyTowerVisuals : MonoBehaviour, ITowerVisuals
{
    public event Action OnAttackImpact;

    public void Initialize()
    {
        Debug.Log("<color=cyan>[DummyTowerVisuals] Визуал башни готов к работе!</color>");
    }

    public void PlayBuildAnimation()
    {
        throw new NotImplementedException();
    }

    public void PlayShootAnimation()
    {
        // Здесь в будущем DOTween будет сжимать меши, а пока просто лог
        Debug.Log("<color=cyan>[DummyTowerVisuals] БАБАХ! (Проигрывается сочная анимация отдачи)</color>");
    }
}
