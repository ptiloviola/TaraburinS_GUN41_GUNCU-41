using UnityEngine;
using TMPro;
using Zenject;

public class GameUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _turnText;
    [SerializeField] private TextMeshProUGUI _checkText;

    private SignalBus _signal;
    private ITurn _turn;

    [Inject]
    public void Construct(SignalBus signal, ITurn turn)
    {
        _signal = signal;
        _turn = turn;
        _signal.Subscribe<TurnChangedSignal>(UpdateTurnUI);
        _signal.Subscribe<CheckSignal>(OnCheck);
    }

    private void Start()
    {
        _checkText.gameObject.SetActive(false);
        UpdateTurnUI();
        
    }

    private void UpdateTurnUI()
    {
        string teamName = _turn.Current == Team.White ? "<color=#FFFFFF>WHITE</color>" : "<color=#555555>BLACK</color>";

        _turnText.text = $"{teamName} moves!";

        _checkText.gameObject.SetActive(false);
    }

    private void OnCheck(CheckSignal args)
    {
        string teamName = args.TeamInCheck == Team.White ? "WHITE" : "BLACK";
        _checkText.text = $"<color=red>CHECK TO THE {teamName} KING!</color>";
        _checkText.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        _signal.Unsubscribe<TurnChangedSignal>(UpdateTurnUI);
        _signal.Unsubscribe<CheckSignal>(OnCheck);
    }

}
